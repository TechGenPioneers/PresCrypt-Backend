using PresCrypt_Backend.PresCrypt.Core.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AdminContactUsDto
    {
        public string InquiryId { get; set; }

        public string? PatientId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }
        public byte[]? ProfileImage { get; set; }

        public string? Topic { get; set; }

        public string? Description { get; set; }

        public string? SenderType { get; set; }

        public string? ReplyMessage { get; set; }

        public bool? IsRead { get; set; } = false;
    }
}
