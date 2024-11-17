namespace Balance_Support.DataClasses.Records.UserData;

public record UserSettingsUpdateRequest(
    string UserId,
    string UserName,
    string Nickname,
    string PhoneNumber,
    string Address,
    string Country,
    string About,
    bool CommentsOnArticle,
    bool AnswersOnForm,
    bool OnFollower,
    bool NewsAnnouncements,
    bool ProductUpdates,
    bool BlogDigest,
    List<int> SelectedGroups,
    int RowsCount);