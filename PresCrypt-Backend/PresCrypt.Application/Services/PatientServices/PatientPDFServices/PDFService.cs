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
                    page.Margin(0);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                    page.Header().Element(BuildHeader(logoBytes));
                    page.Content().Element(BuildContent(details));
                    page.Footer().Element(BuildFooter(logoBytes));
                });
            });

            return document.GeneratePdf();
        }

        private static Action<IContainer> BuildHeader(byte[] logoBytes) => container =>
        {
            container.Background("#094A4D").Padding(10).Row(row =>
            {
                row.ConstantColumn(100).Image(logoBytes, ImageScaling.FitWidth);

                row.RelativeColumn().AlignMiddle().Column(col =>
                {
                    col.Item().Text("PresCrypt").SemiBold().FontSize(24).FontColor(Colors.White);
                    col.Item().Text("HealthCare Appointment Slip")
                   .Italic().FontSize(14).FontColor("#E0E0E0");
   
                });
            });
        };

        private static Action<IContainer> BuildFooter(byte[] logoBytes) => container =>
        {
            container.Background("#094A4D").Padding(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantColumn(100).Image(logoBytes, ImageScaling.FitWidth);
                    row.RelativeColumn().AlignMiddle().Column(inner =>
                    {
                        inner.Item().Text("Contact Info").FontColor(Colors.White).SemiBold().FontSize(14);
                        inner.Item().Text("Inquiry Hotline: 0762085246").FontColor(Colors.White);
                        inner.Item().Text("Address: Bandaranayake Mawatha, Moratuwa 10400, Sri Lanka.").FontColor(Colors.White);
                        inner.Item().Text("Email: prescrypt.health@gmail.com").FontColor(Colors.White);
                    });
                });
            });
        };

        private static Action<IContainer> BuildContent(AppointmentPDFDetailsDto details) => container =>
        {
            container.Padding(30).Column(x =>
            {
                x.Item().PaddingBottom(10).Row(row =>
                {
                    row.RelativeColumn().Text($"Issued To: {details.PatientId}").WrapAnywhere();
                    row.RelativeColumn().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Payment ID: 01234");
                        col.Item().Text($"Date: {DateTime.Now:dd.MM.yyyy}");
                    });
                });

                AddAppointmentTable(x, details);
                AddChargesTable(x, details);

                x.Item().PaddingTop(20).Text("Please be punctual to avoid any inconvenience. For online payments, no refunds will be issued if you fail to cancel the appointment at least 48 hours before the scheduled time. If you cancel the appointment 48 hours in advance, 80% of the payment will be refunded. For payments made at the hospital, please ensure you pay the amount mentioned in the appointment details document. Account inactivation will be applied to patients who choose “pay at the hospital” but fail to attend the appointment without prior notice to the hospital.")
                    .FontSize(11).LineHeight(1.5f);
            });
        };

        private static void AddAppointmentTable(ColumnDescriptor x, AppointmentPDFDetailsDto details)
        {
            x.Item().PaddingBottom(5).Text("Appointment Details").SemiBold().FontSize(16);

            x.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(1);
                    c.RelativeColumn(2);
                });

                AddTableRow(table, "Patient ID", details.PatientId);
                AddTableRow(table, "Doctor Name", details.DoctorName);
                AddTableRow(table, "Hospital Name", details.HospitalName);
                AddTableRow(table, "Appointment Date", details.AppointmentDate);
                AddTableRow(table, "Appointment Time", details.AppointmentTime);
            });
        }

        private static void AddChargesTable(ColumnDescriptor x, AppointmentPDFDetailsDto details)
        {
            x.Item().PaddingBottom(5).Text("Charges Breakdown").SemiBold().FontSize(16);

            x.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn();
                    c.ConstantColumn(120);
                });

                AddTableRow(table, "Doctor Charges", $"Rs. {details.DoctorCharge:0.00}");
                AddTableRow(table, "Hospital Charges", $"Rs. {details.HospitalCharge:0.00}");
                AddTableRow(table, "Platform Charges", $"Rs. {details.PlatformCharge:0.00}");
                AddTableRow(table, "TOTAL", $"Rs. {details.TotalCharge:0.00}", true);
            });
        }

        private static void AddTableRow(TableDescriptor table, string label, string value, bool bold = false)
        {
            var style = bold ? TextStyle.Default.SemiBold() : TextStyle.Default;

            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(label).Style(style).WrapAnywhere();
            table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(value).Style(style);
        }



        public async Task<byte[]> GeneratePdfAsync(List<PatientAppointmentListDto> appointments)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var logoPath = Path.Combine(_wwwRootPath, "images", "logo.png");
            var logoBytes = File.Exists(logoPath) ? File.ReadAllBytes(logoPath) : Array.Empty<byte>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Helvetica"));

                    page.Header().Element(BuildListHeader(logoBytes));
                    page.Content().Padding(30).Element(container =>
                    {
                        container.Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            table.Cell().Element(CellStyle).Text("Date").SemiBold().FontColor("#094A4D");
                            table.Cell().Element(CellStyle).Text("Doctor").SemiBold().FontColor("#094A4D");
                            table.Cell().Element(CellStyle).Text("Hospital").SemiBold().FontColor("#094A4D");
                            table.Cell().Element(CellStyle).Text("Time").SemiBold().FontColor("#094A4D");
                            table.Cell().Element(CellStyle).Text("Status").SemiBold().FontColor("#094A4D");

                            foreach (var appt in appointments)
                            {
                                table.Cell().Element(CellStyle).Text(appt.Date.ToString("yyyy-MM-dd"));
                                table.Cell().Element(CellStyle).Text(appt.DoctorName);
                                table.Cell().Element(CellStyle).Text(appt.HospitalName);
                                table.Cell().Element(CellStyle).Text(appt.Time);
                                table.Cell().Element(CellStyle).Text(appt.Status);
                            }

                            IContainer CellStyle(IContainer cell) => cell.BorderBottom(1).Padding(5);
                        });
                    });
                    page.Footer().Element(BuildListFooter(logoBytes));
                });
            });

            return document.GeneratePdf();
        }

        private static Action<IContainer> BuildListHeader(byte[] logoBytes) => container =>
        {
            container.Background("#094A4D").Padding(10).Row(row =>
            {
                row.ConstantColumn(100).Image(logoBytes, ImageScaling.FitWidth);

                row.RelativeColumn().AlignMiddle().Column(col =>
                {
                    col.Item().Text("PRESCRYPT").SemiBold().FontSize(24).FontColor(Colors.White);
                    col.Item().Text("Appointment List").Italic().FontSize(14).FontColor("#E0E0E0");
                });
            });
        };

        private static Action<IContainer> BuildListFooter(byte[] logoBytes) => container =>
        {
            container.Background("#094A4D").Padding(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.ConstantColumn(100).Image(logoBytes, ImageScaling.FitWidth);

                    row.RelativeColumn().AlignMiddle().Column(inner =>
                    {
                        inner.Item().Text("Contact Info").FontColor(Colors.White).SemiBold().FontSize(14);
                        inner.Item().Text("Inquiry Hotline: 0762085246").FontColor(Colors.White);
                        inner.Item().Text("Address: Bandaranayake Mawatha, Moratuwa 10400, Sri Lanka.").FontColor(Colors.White);
                        inner.Item().Text("Email: prescrypt.health@gmail.com").FontColor(Colors.White);
                    });
                });
            });
        };




    }
}
