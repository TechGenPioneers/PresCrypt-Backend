using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;
using System.IO;
using Microsoft.EntityFrameworkCore;
using PresCrypt_Backend.PresCrypt.Core.Models;
using PresCrypt_Backend.PresCrypt.API.Dto;
using Microsoft.Extensions.Logging;

namespace PresCrypt_Backend.PresCrypt.Application.Services.DoctorPatientServices
{
    public class DoctorReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DoctorReportService> _logger;

        public DoctorReportService(ApplicationDbContext context, ILogger<DoctorReportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<byte[]> GenerateReportAsync(DoctorReportDto request)
        {
            try
            {
                // Validate request
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (request.From > request.To)
                    throw new ArgumentException("End date cannot be before start date");

                // Get doctor info first
                var doctor = await _context.Doctor
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.DoctorId == request.DoctorId);

                if (doctor == null)
                    throw new KeyNotFoundException($"Doctor with ID {request.DoctorId} not found");

                var doctorName = $"{doctor.FirstName} {doctor.LastName}";

                // Generate the appropriate report based on type
                return request.ReportType switch
                {
                    "Appointments History" => await GenerateAppointmentsHistoryReport(request, doctorName),
                    "Summary Report" => await GenerateSummaryReport(request, doctorName),
                    "Detailed Report" => await GenerateDetailedReport(request, doctorName),
                    _ => throw new ArgumentException($"Invalid report type: {request.ReportType}")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                throw;
            }
        }

        private async Task<byte[]> GenerateAppointmentsHistoryReport(DoctorReportDto request, string doctorName)
        {
            try
            {
                _logger.LogInformation($"Starting report generation for doctor {request.DoctorId} from {request.From} to {request.To}");

                // Convert dates safely
                var fromDate = DateOnly.FromDateTime(request.From);
                var toDate = DateOnly.FromDateTime(request.To);

                // Base query with explicit joins for left-join support and DTO projection handling null values
                var query = from appointment in _context.Appointments
                            join patient in _context.Patient
                                on appointment.PatientId equals patient.PatientId into patientGroup
                            from patient in patientGroup.DefaultIfEmpty()
                            join hospital in _context.Hospitals
                                on appointment.HospitalId equals hospital.HospitalId into hospitalGroup
                            from hospital in hospitalGroup.DefaultIfEmpty()
                            where appointment.DoctorId == request.DoctorId &&
                                  appointment.Date >= fromDate &&
                                  appointment.Date <= toDate
                            select new
                            {
                                Appointment = appointment,
                                Patient = patient,
                                Hospital = hospital
                            };

                // Filter by patient if specified
                if (!string.IsNullOrEmpty(request.Patient) && request.Patient != "all")
                {
                    query = query.Where(x => x.Appointment.PatientId == request.Patient);
                }

                var results = await query.AsNoTracking().ToListAsync();

                _logger.LogInformation($"Found {results.Count()} appointments for processing");

                // Handle case when no appointments found
                if (!results.Any())
                {
                    _logger.LogWarning("No appointments found for the specified criteria");
                    return GenerateEmptyReport(doctorName, request, "Appointments History");
                }

                // Safely group by patient
                var appointmentsByPatient = results
                    .GroupBy(x => new
                    {
                        PatientId = x.Appointment.PatientId ?? "unknown",
                        FirstName = x.Patient?.FirstName ?? "Unknown",
                        LastName = x.Patient?.LastName ?? "Patient"
                    })
                    .Select(g => new
                    {
                        PatientName = $"{g.Key.FirstName} {g.Key.LastName}",
                        PatientId = g.Key.PatientId,
                        AppointmentCount = g.Count(),
                        LastAppointment = g.Max(x => x.Appointment.Date)
                    })
                    .OrderByDescending(x => x.AppointmentCount)
                    .ToList();

                // Safely group by hospital
                var appointmentsByHospital = results
                    .GroupBy(x => new
                    {
                        HospitalId = x.Appointment.HospitalId ?? "unknown",
                        HospitalName = x.Hospital?.HospitalName ?? "Unknown Hospital"
                    })
                    .Select(g => new
                    {
                        HospitalName = g.Key.HospitalName,
                        HospitalId = g.Key.HospitalId,
                        AppointmentCount = g.Count()
                    })
                    .OrderByDescending(x => x.AppointmentCount)
                    .ToList();

                

                // Generate PDF
                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);

                    // Add logo and get its height
                    double logoHeight = await AddLogoToPdf(gfx, page);

                    // Calculate starting Y position after logo
                    double yPos = 30 + logoHeight + 20; // Y (30) + logoHeight + 20px gap


                    var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                    var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
                    var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);

                    

                    // Report title
                    gfx.DrawString("Appointment History Report", fontTitle, XBrushes.Black,
                        new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                    yPos += 30;

                    // Doctor info
                    gfx.DrawString($"Doctor: {doctorName} (ID: {request.DoctorId})", fontHeader, XBrushes.Black,
                        new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    yPos += 20;

                    // Date range
                    gfx.DrawString($"Period: {request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}",
                        fontHeader, XBrushes.Black,
                        new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    yPos += 30;

                    // Patient filter info
                    string patientFilter = request.Patient == "all"
                        ? "All Patients"
                        : $"Patient: {appointmentsByPatient.FirstOrDefault()?.PatientName ?? "N/A"}";

                    gfx.DrawString(patientFilter, fontHeader, XBrushes.Black,
                        new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    yPos += 20;

                    // Summary statistics
                    gfx.DrawString($"Total Appointments: {results.Count()}", fontHeader, XBrushes.Black,
                        new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    yPos += 30;

                    // Appointments by patient table (only if showing all patients)
                    if (request.Patient == "all" && appointmentsByPatient.Any())
                    {
                        yPos = DrawTable(gfx, yPos, fontHeader, fontNormal,
                            "Appointments by Patient",
                            new[] { "Patient Name", "Patient ID", "Appointments", "Last Visit" },
                            appointmentsByPatient.Select(x => new[] {
                        x.PatientName,
                        x.PatientId,
                        x.AppointmentCount.ToString(),
                        x.LastAppointment.ToString("yyyy-MM-dd")
                            }).ToList());
                    }

                    // Appointments by hospital table
                    if (appointmentsByHospital.Any())
                    {
                        yPos = DrawTable(gfx, yPos, fontHeader, fontNormal,
                            "Appointments by Hospital",
                            new[] { "Hospital Name", "Hospital ID", "Appointments" },
                            appointmentsByHospital.Select(x => new[] {
                        x.HospitalName,
                        x.HospitalId,
                        x.AppointmentCount.ToString()
                            }).ToList());
                    }

                    // Save to memory stream
                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream, false);
                        _logger.LogInformation("Successfully generated PDF report");
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating appointments history report");
                return GenerateErrorReport(doctorName, request, "Appointments History",
                    $"Error: {ex.Message}. Please check the data and try again.");
            }
        }

        private async Task<byte[]> GenerateSummaryReport(DoctorReportDto request, string doctorName)
        {
            try
            {
                // Base query
                var query = _context.Appointments
                    .Where(a => a.DoctorId == request.DoctorId &&
                               a.Date != null &&
                               a.Date >= DateOnly.FromDateTime(request.From) &&
                               a.Date <= DateOnly.FromDateTime(request.To));

                // Filter by patient if specified
                if (!string.IsNullOrEmpty(request.Patient) && request.Patient != "all")
                {
                    query = query.Where(a => a.PatientId == request.Patient);
                }

                var stats = await query
                    .GroupBy(a => 1)
                    .Select(g => new
                    {
                        TotalAppointments = g.Count(),
                        UniquePatients = request.Patient == "all"
                            ? g.Select(a => a.PatientId).Distinct().Count()
                            : 1, // If specific patient, count as 1
                        CancelledAppointments = g.Count(a => a.Status == "Cancelled"),
                        RescheduledAppointments = g.Count(a => a.Status == "Rescheduled"),
                        FirstAppointment = g.Min(a => a.Date),
                        LastAppointment = g.Max(a => a.Date)
                    })
                    .FirstOrDefaultAsync();

                using (var document = new PdfDocument())
                {
                    var page = document.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);
                    var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                    var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
                    var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);

                    double yPos = 50;

                    // Report title
                    string reportTitle = request.Patient == "all"
                        ? "Summary Report - All Patients"
                        : $"Summary Report - Patient ID: {request.Patient}";

                    gfx.DrawString(reportTitle, fontTitle, XBrushes.Black,
                        new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                    yPos += 30;

                    // Doctor info
                    gfx.DrawString($"Doctor: {doctorName} (ID: {request.DoctorId})", fontHeader, XBrushes.Black,
                        new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    yPos += 20;

                    // Date range
                    gfx.DrawString($"Period: {request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}",
                        fontHeader, XBrushes.Black,
                        new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    yPos += 30;

                    // Summary statistics
                    if (stats != null)
                    {
                        var summaryData = new List<string[]>
                {
                    new[] { "Total Appointments", stats.TotalAppointments.ToString() },
                    new[] { "No of Patients", stats.UniquePatients.ToString() },
                    new[] { "Cancelled Appointments", stats.CancelledAppointments.ToString() },
                    new[] { "Rescheduled Appointments", stats.RescheduledAppointments.ToString() },
                    new[] { "First Appointment", stats.FirstAppointment.ToString("yyyy-MM-dd") },
                    new[] { "Last Appointment", stats.LastAppointment.ToString("yyyy-MM-dd") }
                };

                        yPos = DrawTable(gfx, yPos, fontHeader, fontNormal,
                            "Practice Summary",
                            new[] { "Metric", "Value" },
                            summaryData);
                    }
                    else
                    {
                        gfx.DrawString("No appointment data found for the selected period",
                            fontHeader, XBrushes.Black,
                            new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                    }

                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream, false);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating summary report");
                throw;
            }
        }

        private async Task<byte[]> GenerateDetailedReport(DoctorReportDto request, string doctorName)
        {
            try
            {
                // Base query
                var query = _context.Appointments
                    .Where(a => a.DoctorId == request.DoctorId &&
                               a.Date != null &&
                               a.Date >= DateOnly.FromDateTime(request.From) &&
                               a.Date <= DateOnly.FromDateTime(request.To))
                    .OrderBy(a => a.Date);

                // Filter by patient if specified
                if (!string.IsNullOrEmpty(request.Patient) && request.Patient != "all")
                {
                    query = query.Where(a => a.PatientId == request.Patient).OrderBy(a => a.Date);
                }

                var appointmentData = await query
                    .Select(a => new
                    {
                        a.AppointmentId,
                        a.Date,
                        PatientName = a.Patient != null ? $"{a.Patient.FirstName} {a.Patient.LastName}" : "Unknown Patient",
                        PatientId = a.PatientId ?? "N/A",
                        HospitalName = a.Hospital != null ? a.Hospital.HospitalName : "N/A",
                        Status = a.Status ?? "Unknown"
                    })
                    .AsNoTracking()
                    .ToListAsync();

                if (!appointmentData.Any())
                {
                    return GenerateEmptyReport(doctorName, request,
                        request.Patient == "all"
                            ? "Detailed Appointment Report - All Patients"
                            : $"Detailed Appointment Report - Patient ID: {request.Patient}");
                }

                // Create PDF document
                using (var document = new PdfDocument())
                {
                    // Define fonts and styles
                    var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                    var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);
                    var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);

                    // Constants for layout
                    const double topMargin = 50;
                    const double bottomMargin = 50;
                    const double rowHeight = 20;
                    double yPos = topMargin;
                    int currentPage = 0;
                    PdfPage page = document.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    currentPage++;

                    // Draw header (repeated on each page)
                    void DrawHeader(bool isFirstPage)
                    {
                        // Add logo
                        double logoHeight = AddLogoToPdf(gfx, page).Result;
                        yPos = 30 + logoHeight + 20;

                        // Report title
                        string title = isFirstPage
                            ? request.Patient == "all"
                                ? "Detailed Appointment Report - All Patients"
                                : $"Detailed Appointment Report - Patient ID: {request.Patient}"
                            : request.Patient == "all"
                                ? "Detailed Appointment Report - All Patients (cont.)"
                                : $"Detailed Appointment Report - Patient ID: {request.Patient} (cont.)";

                        gfx.DrawString(title, fontTitle, XBrushes.Black,
                            new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                        yPos += 30;

                        // Doctor info
                        gfx.DrawString($"Doctor: {doctorName} (ID: {request.DoctorId})", fontHeader, XBrushes.Black,
                            new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                        yPos += 20;

                        // Date range
                        gfx.DrawString($"Period: {request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}",
                            fontHeader, XBrushes.Black,
                            new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                        yPos += 30;
                    }

                    // Prepare table data
                    var tableData = appointmentData.Select(a => new[] {
                a.Date.ToString("yyyy-MM-dd"),
                a.PatientName,
                a.PatientId,
                a.HospitalName,
                a.Status
            }).ToList();

                    // Table drawing with pagination
                    int currentRow = 0;
                    bool isFirstPage = true;

                    while (currentRow < tableData.Count)
                    {
                        // Draw header for each page
                        DrawHeader(isFirstPage);
                        isFirstPage = false;

                        // Calculate available space
                        double availableHeight = page.Height - yPos - bottomMargin;
                        int rowsPerPage = (int)(availableHeight / rowHeight) - 1; // -1 for header row

                        // Ensure we have at least one row per page
                        rowsPerPage = Math.Max(rowsPerPage, 1);

                        // Get rows for current page
                        int rowsToTake = Math.Min(rowsPerPage, tableData.Count - currentRow);
                        var pageData = tableData.Skip(currentRow).Take(rowsToTake).ToList();

                        // Draw table section
                        yPos = DrawTable(gfx, yPos, fontHeader, fontNormal,
                            currentRow == 0 ? "Appointment Details" : "",
                            new[] { "Date", "Patient", "Patient ID", "Hospital", "Status" },
                            pageData,
                            new double[] { 80, 120, 80, 150, 80 });

                        currentRow += rowsToTake;

                        // Create new page if more rows remain
                        if (currentRow < tableData.Count)
                        {
                            page = document.AddPage();
                            gfx = XGraphics.FromPdfPage(page);
                            yPos = topMargin;
                            currentPage++;
                        }
                    }

                    // Save to memory stream
                    using (var stream = new MemoryStream())
                    {
                        document.Save(stream, false);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating detailed report");
                return GenerateErrorReport(doctorName, request,
                    request.Patient == "all"
                        ? "Detailed Report - All Patients"
                        : $"Detailed Report - Patient ID: {request.Patient}",
                    $"Error generating report: {ex.Message}");
            }
        }

        private byte[] GenerateEmptyReport(string doctorName, DoctorReportDto request, string reportTitle)
        {
            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontHeader = new XFont("Arial", 12, XFontStyle.Bold);

                double yPos = 50;

                // Report title
                gfx.DrawString(reportTitle, fontTitle, XBrushes.Black,
                    new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                yPos += 30;

                // Doctor info
                gfx.DrawString($"Doctor: {doctorName} (ID: {request.DoctorId})", fontHeader, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                yPos += 20;

                // Date range
                gfx.DrawString($"Period: {request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}",
                    fontHeader, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                yPos += 30;

                // Empty data message
                gfx.DrawString("No data found for the selected criteria",
                    fontHeader, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);

                using (var stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }

        private double DrawTable(XGraphics gfx, double yPos, XFont fontHeader, XFont fontNormal,
        string title, string[] headers, List<string[]> rows, double[] columnWidths = null)
        {
            const double rowHeight = 20;
            const double margin = 50;

            // Use custom widths if provided, otherwise calculate equal widths
            double[] widths = columnWidths ?? new double[headers.Length];
            if (columnWidths == null)
            {
                double equalWidth = (gfx.PageSize.Width - margin * 2) / headers.Length;
                for (int i = 0; i < headers.Length; i++)
                {
                    widths[i] = equalWidth;
                }
            }

            // Calculate total table width
            double tableWidth = widths.Sum();

            // Draw table title if provided
            if (!string.IsNullOrEmpty(title))
            {
                gfx.DrawString(title, fontHeader, XBrushes.Black,
                    new XRect(margin, yPos, tableWidth, rowHeight),
                    XStringFormats.TopLeft);
                yPos += rowHeight;
            }

            // Draw headers
            double currentX = margin;
            for (int i = 0; i < headers.Length; i++)
            {
                // Header styling
                XColor headerBgColor = XColor.FromArgb(0xE9, 0xFA, 0xF2);
                XColor headerTextColor = XColor.FromArgb(0x09, 0x4A, 0x4D);

                // Draw header cell
                gfx.DrawRectangle(new XSolidBrush(headerBgColor),
                    currentX, yPos, widths[i], rowHeight);
                gfx.DrawRectangle(XPens.Black,
                    currentX, yPos, widths[i], rowHeight);

                // Draw header text
                gfx.DrawString(headers[i], fontHeader, new XSolidBrush(headerTextColor),
                    new XRect(currentX + 5, yPos, widths[i] - 10, rowHeight),
                    XStringFormats.CenterLeft);

                currentX += widths[i];
            }
            yPos += rowHeight;

            // Draw rows
            foreach (var row in rows)
            {
                currentX = margin;
                for (int i = 0; i < row.Length; i++)
                {
                    // Draw cell background and border
                    gfx.DrawRectangle(XBrushes.White, currentX, yPos, widths[i], rowHeight);
                    gfx.DrawRectangle(XPens.Black, currentX, yPos, widths[i], rowHeight);

                    // Draw cell content
                    gfx.DrawString(row[i], fontNormal, XBrushes.Black,
                        new XRect(currentX + 5, yPos, widths[i] - 10, rowHeight),
                        XStringFormats.CenterLeft);

                    currentX += widths[i];
                }
                yPos += rowHeight;
            }

            return yPos + 20; // Add some spacing after the table
        }


        private byte[] GenerateErrorReport(string doctorName, DoctorReportDto request, string reportType, string errorMessage)
        {
            using (var document = new PdfDocument())
            {
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var fontTitle = new XFont("Arial", 16, XFontStyle.Bold);
                var fontNormal = new XFont("Arial", 12, XFontStyle.Regular);

                double yPos = 50;

                gfx.DrawString($"{reportType} - Error", fontTitle, XBrushes.Black,
                    new XRect(0, yPos, page.Width, page.Height), XStringFormats.TopCenter);
                yPos += 30;

                gfx.DrawString($"Doctor: {doctorName}", fontNormal, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                yPos += 20;

                gfx.DrawString($"Period: {request.From:yyyy-MM-dd} to {request.To:yyyy-MM-dd}",
                    fontNormal, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                yPos += 30;

                gfx.DrawString("Error generating report:", fontNormal, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);
                yPos += 20;

                gfx.DrawString(errorMessage, fontNormal, XBrushes.Black,
                    new XRect(50, yPos, page.Width - 100, page.Height), XStringFormats.TopLeft);

                using (var stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }

        private async Task<double> AddLogoToPdf(XGraphics gfx, PdfPage page) // Changed to return double
        {
            try
            {
                // Local file path to the logo
                string localFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "logo.png");

                // Read logo bytes from local file system
                byte[] logoBytes = File.ReadAllBytes(localFilePath);
                using (var ms = new MemoryStream(logoBytes))
                {
                    XImage logo = XImage.FromStream(() => ms);

                    // Define logo dimensions
                    double logoWidth = 100; // Fixed width
                    double logoHeight = logo.PixelHeight * (logoWidth / logo.PixelWidth); // Maintain aspect ratio

                    // Center horizontally
                    double centerX = (page.Width - logoWidth) / 2;

                    // Draw logo at top-center (Y position: 30)
                    gfx.DrawImage(logo, centerX, 30, logoWidth, logoHeight);

                    return logoHeight; // Return the actual height used
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Logo load failed: {ex.Message}");
                return 0; // Return 0 if logo failed to load
            }
        }

    }
}