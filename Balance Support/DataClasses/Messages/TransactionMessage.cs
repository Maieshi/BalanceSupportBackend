namespace Balance_Support.DataClasses.Messages;

public class TransactionMessage : IMessage
{
    public string AccountId { get; set; }
    public string LastName { get; set; }
    public string Message { get; set; }
    public string DeviceId { get; set; }
    public string SmsTime { get; set; }
    public string SmsDate { get; set; }
    public string CardNumber { get; set; }
    public string BankType { get; set; }
    public string Channel { get; set; }
    public bool Incoming { get; set; }
    public bool Outgoing { get; set; }
}
