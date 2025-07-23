using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Payment
    {
        [Key]
        [Required]
        public string PaymentId { get; set; }

        public double PaymentAmount { get; set; }

        public string PaymentMethod { get; set; }

        public string PaymentStatus { get; set; }

        public bool ? IsRefunded { get; set; } = false;

        public string? PayHereObjectId { get; set; }


    }
}
