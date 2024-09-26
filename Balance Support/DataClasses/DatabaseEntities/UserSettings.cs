 using System.ComponentModel.DataAnnotations.Schema;
 using Balance_Support.DataClasses.DatabaseEntities;
 using Newtonsoft.Json;

 public class UserSettings:BaseEntity
{ 
    [ForeignKey("User")]
    public string UserId { get; set; }

    public int SelectedGroup { get; set; }

    public int RowCount { get; set; }
    [JsonIgnore]
    public virtual User User { get; set; }
    public UserSettings(string userId)
    {
        UserId = userId;
        SelectedGroup = 0;
        RowCount = 0;
    }
}