using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class OpenMrsPatientCreateDto
    {
        [Required]
        public string PatientId { get; set; }
    }

    public class OpenMrsPatientResponseDto
    {
        public string OpenMrsId { get; set; }
        public string Identifier { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    // Updated DTO for OpenMRS identifier response
    public class OpenMrsIdentifierResponseDto
    {
        public List<OpenMrsIdentifierResult> Results { get; set; }
    }

    public class OpenMrsIdentifierResult
    {
        public string IdentifierValue { get; set; }
    }

    // Internal DTOs for OpenMRS API
    public class OpenMrsPatientRequestDto
    {
        public List<OpenMrsIdentifierDto> Identifiers { get; set; }
        public OpenMrsPersonDto Person { get; set; }
    }

    public class OpenMrsIdentifierDto
    {
        public string Identifier { get; set; }
        public string IdentifierType { get; set; }
        public OpenMrsLocationDto Location { get; set; }
        public bool Preferred { get; set; }
    }

    public class OpenMrsLocationDto
    {
        public string Uuid { get; set; }
    }

    public class OpenMrsPersonDto
    {
        public string Gender { get; set; }
        public int Age { get; set; }
        public string Birthdate { get; set; }
        public bool BirthdateEstimated { get; set; }
        public bool Dead { get; set; }
        public object DeathDate { get; set; }
        public object CauseOfDeath { get; set; }
        public List<OpenMrsNameDto> Names { get; set; }
        public List<OpenMrsAddressDto> Addresses { get; set; }
    }

    public class OpenMrsNameDto
    {
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }

    public class OpenMrsAddressDto
    {
        public string Address1 { get; set; }
        public string CityVillage { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class OpenMrsPatientCreatedResponseDto
    {
        public string Uuid { get; set; }
        public List<OpenMrsIdentifierDto> Identifiers { get; set; }
        public OpenMrsPersonDto Person { get; set; }
    }
}