namespace Balance_Support.Scripts.Extensions;

public  class DefaultContextStrategy: ContextStrategy
{
    public override bool IsUserAuthorized(HttpContext httpContext)
    {
        if (httpContext == null || httpContext.User == null || !httpContext.User.Identity.IsAuthenticated)
        {
            return false; 
        }
        var user = httpContext.User;
        var sessionStartTime = DateTime.MinValue;
        var expiresUtc = DateTime.MinValue;

        if (user.Identity.IsAuthenticated && user.HasClaim(c => c.Type == "FirebaseToken"))
        {
            var sessionStartClaim = user.FindFirst("SessionStartTime");
            var expiresUtcClaim = user.FindFirst("ExpiresUtc");

            if (sessionStartClaim != null && DateTime.TryParse(sessionStartClaim.Value, out sessionStartTime) &&
                expiresUtcClaim != null && DateTime.TryParse(expiresUtcClaim.Value, out expiresUtc))
            {
                // Compare the current time with the expiration time
                if (DateTime.UtcNow <= expiresUtc)
                {
                    return true; // User is authorized
                }
            }
        }

        return false;
    }
    
}