namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class PatientAppointmentEmailDto
    {
        public string Receptor { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }

        // NEW attachment support
        public EmailAttachmentDto Attachment { get; set; }
    }

    public class EmailAttachmentDto
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Base64Content { get; set; }
    }
}
