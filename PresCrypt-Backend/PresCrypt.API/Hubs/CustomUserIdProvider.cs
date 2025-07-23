using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PresCrypt_Backend.PresCrypt.API.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // If the user is authenticated, prefer the NameIdentifier claim
            var userIdFromClaim = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdFromClaim))
            {
                return userIdFromClaim;
            }

            // Fallback to query string (for unauthenticated scenario)
            var userIdFromQuery = connection.GetHttpContext()?.Request.Query["userId"];
            return userIdFromQuery;
        }
    }
}
