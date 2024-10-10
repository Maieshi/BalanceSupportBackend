using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.UserData;
using Newtonsoft.Json;

namespace Balance_Support.DataClasses.DatabaseEntities;
public class UserSettings : BaseEntity
{
    public UserSettings(string userId)
    {
        UserId = userId;

        UserName = string.Empty;
        Nickname = string.Empty;
        PhoneNumber = string.Empty;
        Address = string.Empty;
        Country = string.Empty;
        About = string.Empty;

        CommentsOnArticle = false;
        AnswersOnForm = false;
        OnFollower = false;
        NewsAnnouncements = false;
        ProductUpdates = false;
        BlogDigest = false;

        SelectedGroup = 0;
        RowsCount = 0;
    }


    [ForeignKey("User")] public string UserId { get; set; }

    [StringLength(50)] public string UserName { get; set; }

    [StringLength(50)] public string Nickname { get; set; }

    [StringLength(20)] public string PhoneNumber { get; set; }

    [StringLength(200)] public string Address { get; set; }

    [StringLength(100)] public string Country { get; set; }

    [StringLength(500)] public string About { get; set; }

    public bool CommentsOnArticle { get; set; }

    public bool AnswersOnForm { get; set; }

    public bool OnFollower { get; set; }

    public bool NewsAnnouncements { get; set; }

    public bool ProductUpdates { get; set; }

    public bool BlogDigest { get; set; }

    public int SelectedGroup { get; set; }

    public int RowsCount { get; set; }

    [JsonIgnore] public virtual User User { get; set; }

    public void Update(UserSettingsUpdateRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        UserName = request.UserName;
        Nickname = request.Nickname;
        PhoneNumber = request.PhoneNumber;
        Address = request.Address;
        Country = request.Country;
        About = request.About;

        CommentsOnArticle = request.CommentsOnArticle;
        AnswersOnForm = request.AnswersOnForm;
        OnFollower = request.OnFollower;
        NewsAnnouncements = request.NewsAnnouncements;
        ProductUpdates = request.ProductUpdates;
        BlogDigest = request.BlogDigest;

        SelectedGroup = request.SelectedGroup;
        RowsCount = request.RowsCount;
    }
}