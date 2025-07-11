using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using PresCrypt_Backend.PresCrypt.API.Dto;
using static PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices.PDFService;

namespace PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices
{
    public class PDFService : IPDFService
    {
        private readonly string _wwwRootPath;

        public PDFService(IWebHostEnvironment env)
        {
            _wwwRootPath = env.WebRootPath;
        }

        public byte[] GeneratePDF(AppointmentPDFDetailsDto details)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var filePath = Path.Combine(_wwwRootPath, "images", "logo.png");
            var logoBytes = File.Exists(filePath) ? File.ReadAllBytes(filePath) : Array.Empty<byte>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                    page.Header().Element(BuildHeader(logoBytes));
                    page.Content().Element(BuildContent(details));
                    page.Footer().AlignCenter().Text("Thank you for using Prescrypt.").FontSize(10).Italic();
                });
            });

            return document.GeneratePdf();
        }

        private static Action<IContainer> BuildHeader(byte[] logoBytes) => container =>
        {
            container.Row(row =>
            {
                //row.ConstantColumn(80).Padding(2).Image(logoBytes, ImageScaling.FitHeight).MaxHeight(50);

                row.RelativeColumn().Column(col =>
                {
                    col.Item().Text("PRESCRYPT")
                        .SemiBold().FontSize(24).FontColor(Colors.Green.Medium);

                    col.Item().Text("HealthCare Invoice")
                        .Italic().FontSize(14);
                });
            });
        };

        private static Action<IContainer> BuildContent(AppointmentPDFDetailsDto details) => container =>
        {
            container.Column(x =>
            {
                // Meta Info
                x.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeColumn().Text($"Issued To: {details.PatientId}").WrapAnywhere();

                    row.RelativeColumn().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Payment ID: 01234");
                        col.Item().Text($"Date: {DateTime.Now:dd.MM.yyyy}");
                    });
                });

                // Appointment Info
                x.Item().PaddingBottom(5).Text("Appointment Details").SemiBold().FontSize(16);

                x.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.RelativeColumn();
                    });

                    void HeaderCell(string label) =>
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(5)
                            .Text(label).SemiBold().WrapAnywhere();

                    void DataCell(string value) =>
                        table.Cell().Padding(5).Text(value).WrapAnywhere();

                    HeaderCell("Patient ID"); DataCell(details.PatientId);
                    HeaderCell("Doctor Name"); DataCell(details.DoctorName);
                    HeaderCell("Hospital Name"); DataCell(details.HospitalName);
                    HeaderCell("Appointment Date"); DataCell(details.AppointmentDate);
                    HeaderCell("Appointment Time"); DataCell(details.AppointmentTime);
                });

                // Charges Info
                x.Item().PaddingBottom(5).Text("Charges Breakdown").SemiBold().FontSize(16);

                x.Item().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn();
                        c.ConstantColumn(100);
                    });

                    void Row(string label, decimal amount, bool bold = false)
                    {
                        var style = bold ? TextStyle.Default.SemiBold() : TextStyle.Default;
                        table.Cell().Text(label).Style(style).WrapAnywhere();
                        table.Cell().Text($"Rs. {amount:0.00}").Style(style);
                    }

                    Row("Doctor Charges", (decimal)details.DoctorCharge);
                    Row("Hospital Charges", (decimal)details.HospitalCharge);
                    Row("Platform Charges", (decimal)details.PlatformCharge);
                    Row("TOTAL", details.TotalCharge, bold: true);
                });

                // Final Note
                x.Item().PaddingTop(20).Text("This is a computer-generated document.").Italic().FontSize(10);
            });
        };



        public async Task<byte[]> GeneratePdfAsync(List<PatientAppointmentListDto> appointments)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Appointments Report").FontSize(20).SemiBold().FontColor(Colors.Green.Medium);

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(); // Date
                            c.RelativeColumn(); // Doctor
                            c.RelativeColumn(); // Hospital
                            c.RelativeColumn(); // Time
                            c.RelativeColumn(); // Status
                        });

                        // Table Header
                        table.Cell().Element(CellStyle).Text("Date").SemiBold();
                        table.Cell().Element(CellStyle).Text("Doctor").SemiBold();
                        table.Cell().Element(CellStyle).Text("Hospital").SemiBold();
                        table.Cell().Element(CellStyle).Text("Time").SemiBold();
                        table.Cell().Element(CellStyle).Text("Status").SemiBold();


                        foreach (var appt in appointments)
                        {
                            table.Cell().Element(CellStyle).Text(appt.Date.ToString("yyyy-MM-dd"));
                            table.Cell().Element(CellStyle).Text(appt.DoctorName);
                            table.Cell().Element(CellStyle).Text(appt.HospitalName);
                            table.Cell().Element(CellStyle).Text(appt.Time);
                            table.Cell().Element(CellStyle).Text(appt.Status);
                        }

                        IContainer CellStyle(IContainer container) =>
                            container.BorderBottom(1).Padding(5);
                    });

                    page.Footer().AlignCenter().Text("Generated by PresCrypt").FontSize(10).Italic();
                });
            });

            return document.GeneratePdf();
        }



    }
}
