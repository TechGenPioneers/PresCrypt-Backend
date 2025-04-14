public class DoctorPrescriptionDto
{
    public string PatientId { get; set; }  //patient id from frontend

    public string EncounterTypeUuid { get; set; }  //categorize what kind of medical interaction

    public string LocationUuid { get; set; }  //identifier of the location - hospital

    public string? PrescriptionText { get; set; }  // For text-based prescriptions

    public IFormFile? PrescriptionFile { get; set; }  // For file uploads fro prescriptions
}
