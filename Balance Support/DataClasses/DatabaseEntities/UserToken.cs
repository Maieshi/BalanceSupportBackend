using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_Support.DataClasses.DatabaseEntities;

public class UserToken : BaseEntity
{
    [StringLength(250)]
    public string Token { get; set; }

    [ForeignKey("User")]
    public string UserId { get; set; }  // Foreign key to User

    public User User { get; set; } // Navigation property
}
