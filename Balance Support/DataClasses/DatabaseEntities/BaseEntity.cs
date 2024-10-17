using System.ComponentModel.DataAnnotations;

namespace Balance_Support.DataClasses.DatabaseEntities;

public abstract class BaseEntity: IDtoConvertable
{
    [Key] public string Id { get; set; } // Primary key for all derived entities

    public abstract object Convert();
}