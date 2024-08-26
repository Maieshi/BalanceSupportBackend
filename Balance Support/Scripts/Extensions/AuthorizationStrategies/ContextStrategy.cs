namespace Balance_Support.Scripts.Extensions;

public abstract class ContextStrategy
{
    public abstract bool IsUserAuthorized(HttpContext context);
}