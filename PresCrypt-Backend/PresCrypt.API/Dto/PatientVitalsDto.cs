using System.ComponentModel.DataAnnotations;

namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class PatientVitalsDto
    {
        [Required]
        public string PatientId { get; set; }

        [Range(0, 300)]
        public double? Height { get; set; }

        [Range(0, 500)]
        public double? Weight { get; set; }

        public string? BloodGroup { get; set; }

        [Range(0, 1000)]
        public double? BloodSugar { get; set; }

        [Range(0, 200)]
        public int? HeartRate { get; set; }

        [Range(0, 300)]
        public int? SystolicBloodPressure { get; set; }

        [Range(0, 200)]
        public int? DiastolicBloodPressure { get; set; }

        public string? Allergies { get; set; }
    }

    public class OpenMrsObsDto
    {
        public string Person { get; set; }
        public string Concept { get; set; }
        public string ObsDatetime { get; set; }
        public object Value { get; set; }
    }
}
