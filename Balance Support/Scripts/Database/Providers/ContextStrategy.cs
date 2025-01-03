namespace Balance_Support.Scripts.Extensions.AuthorizationStrategies;

public abstract class ContextStrategy
{
    public abstract bool IsUserAuthorized(HttpContext context);
}