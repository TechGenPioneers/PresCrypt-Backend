using PresCrypt_Backend.PresCrypt.API.Dto;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace PresCrypt_Backend.PresCrypt.Application.Services.PatientServices.PatientPDFServices
{
    public class PDFService : IPDFService
    {
        public byte[] GeneratePDF(AppointmentPDFDetailsDto details)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var document = CreateDocument(details);
            return document.GeneratePdf();
        }

        private IDocument CreateDocument(AppointmentPDFDetailsDto details)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(16).FontColor(Colors.Black));

                    page.Header().Text("Appointment Confirmation")
                        .SemiBold().FontSize(28).FontColor(Colors.Blue.Medium);

                    page.Content().PaddingVertical(10)
                        .Column(x =>
                        {
                            x.Spacing(10);
                            x.Item().Text($"Patient ID: {details.PatientId}");
                            x.Item().Text($"Doctor: Dr. {details.DoctorName}");
                            x.Item().Text($"Hospital: {details.HospitalName}");
                            x.Item().Text($"Date: {details.AppointmentDate}");
                            x.Item().Text($"Time: {details.AppointmentTime}");
                            x.Item().Text($"Total Charge: Rs. {details.TotalCharge}");
                        });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

        }
    }
}
