using System.ComponentModel.DataAnnotations;
namespace Balance_Support.DataClasses;

public class BaseEntity
{
    [Key]
    public string Id { get; set; }  // Primary key for all derived entities
}