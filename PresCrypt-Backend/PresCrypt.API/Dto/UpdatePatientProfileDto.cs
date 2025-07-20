using System;
using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.DTOs
{
    public class UpdatePatientProfileDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Birth date is required.")]
        public DateTime BirthDate { get; set; }

        [StringLength(10, ErrorMessage = "Gender cannot be longer than 10 characters.")]
        public string? Gender { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "NIC is required.")]
        [StringLength(20, ErrorMessage = "NIC cannot be longer than 20 characters.")]
        public string Nic { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(255, ErrorMessage = "Address cannot be longer than 255 characters.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string Phone { get; set; }

        /// <summary>
        /// The profile image encoded as a Base64 string.
        /// This field is optional. If null or empty, the existing image will be kept.
        /// </summary>
        public string? ProfileImageBase64 { get; set; }
    }
}