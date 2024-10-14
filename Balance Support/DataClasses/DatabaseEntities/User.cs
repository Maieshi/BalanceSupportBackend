using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Balance_Support.DataClasses.DatabaseEntities;

public class User : BaseEntity
{
    [StringLength(50)] public string DisplayName { get; set; }

    [StringLength(50)] public string Email { get; set; }
    [JsonIgnore]
    public ICollection<Account> Accounts { get; set; } // Navigation property
    [JsonIgnore]
    public UserSettings UserSettings { get; set; }
    [JsonIgnore]
    public ICollection<Transaction> Transactions { get; set; }
}