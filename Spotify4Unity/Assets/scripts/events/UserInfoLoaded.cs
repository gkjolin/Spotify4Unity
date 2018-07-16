public class UserInfoLoaded : GameEventBase
{
    public UserInfo Info { get; set; }
    public UserInfoLoaded(UserInfo info)
    {
        Info = info;
    }
}
