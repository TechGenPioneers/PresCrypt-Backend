namespace PresCrypt_Backend.PresCrypt.API.Dto
{
    public class AppointmentSummaryDashboardDto
    {
            public string? NearestPendingAppointmentDate { get; set; }
            public string? LatestCompletedAppointmentDate { get; set; }
            public int TodayAppointmentCount { get; set; }
     

    }
}
