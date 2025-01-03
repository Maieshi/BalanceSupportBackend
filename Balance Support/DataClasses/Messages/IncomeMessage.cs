using Balance_Support.Scripts.WebSockets.Interfaces;

namespace Balance_Support.DataClasses.Messages;

public class IncomeMessage : IMessage
{
    public decimal BalanceTotal { get; set; }
    public decimal DailyExpression { get; set; }
    public string AccountId { get; set; }
    public decimal T { get; set; }
    public decimal D { get; set; }
    public decimal Balance { get; set; }
}