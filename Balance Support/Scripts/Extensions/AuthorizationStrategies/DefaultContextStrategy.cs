using System.Security.Claims;

namespace Balance_Support.Scripts.Extensions.AuthorizationStrategies;

public  class DefaultContextStrategy: ContextStrategy
{
    public override bool IsUserAuthorized(HttpContext httpContext)
    {
        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            // User is authenticated, you can access the claims
            var username = httpContext.User.Identity.Name; // Get the username from the claim
            var roles = httpContext.User.FindAll(ClaimTypes.Role); // Get user's roles if necessary

            return true;
        }
        else
        {
            // User is not authenticated
            return false;
        }
        //if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
        //{
        //    return false; 
        //}
        //var user = httpContext.User;
        //var sessionStartTime = DateTime.MinValue;
        //var expiresUtc = DateTime.MinValue;

        //if (user.Identity.IsAuthenticated && user.HasClaim(c => c.Type == "FirebaseToken"))
        //{
        //    var sessionStartClaim = user.FindFirst("SessionStartTime");
        //    var expiresUtcClaim = user.FindFirst("ExpiresUtc");

        //    if (sessionStartClaim != null && DateTime.TryParse(sessionStartClaim.Value, out sessionStartTime) &&
        //        expiresUtcClaim != null && DateTime.TryParse(expiresUtcClaim.Value, out expiresUtc))
        //    {
        //        // Compare the current time with the expiration time
        //        if (DateTime.UtcNow <= expiresUtc)
        //        {
        //            return true; // User is authorized
        //        }
        //    }
        //}

        //return false;
    }
    
}