using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PresCrypt_Backend.PresCrypt.Core.Models
{
    public class Message
    {
        [Key]
        public string Id { get; set; } 

        [Required]
        public string SenderId { get; set; }  
        [Required]
        public string SenderType { get; set; } 

        [Required]
        public string ReceiverId { get; set; }
        [Required]
        public string ReceiverType { get; set; } 

        public string Text { get; set; }
        public byte[]? Image { get; set; }
        public DateTime SendAt { get; set; } 
        public Boolean IsReceived { get; set; } 
        public Boolean IsRead { get; set; } 
    }

}
