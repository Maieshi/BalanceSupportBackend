namespace Balance_Support.DataClasses.Messages;

public class GlobalIncomeMessage : IMessage
{
    public float Balance { get; set; }
    public float DailyExpression { get; set; }
}