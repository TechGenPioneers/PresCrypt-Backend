using Microsoft.AspNetCore.SignalR;

public class QueryStringPatientIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        return connection.GetHttpContext()?.Request.Query["patientId"];
    }
}
