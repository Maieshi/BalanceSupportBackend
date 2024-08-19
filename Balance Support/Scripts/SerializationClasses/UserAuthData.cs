namespace Balance_Support.SerializationClasses;

public class UserAuthData
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    
    public bool IsOnlineMobile { get; set; }
    
    public bool IsOnlineDesktop { get; set; }
    
}