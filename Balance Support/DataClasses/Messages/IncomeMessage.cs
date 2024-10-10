using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.DataClasses.Messages;

public class IncomeMessage : IMessage
{
    public float Balance { get; set; }
    public float DailyExpression { get; set; }
    public string AccountId { get; set; }
    public float T { get; set; }
    public float D { get; set; }
}