using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.DataClasses.Messages;

public class AccountIncomeMessage : IMessage
{
    public string AccountId { get; set; }
    public float T { get; set; }
    public float D { get; set; }
}