public class MuteChanged : GameEventBase
{
    public bool IsMuted { get; set; }
    public MuteChanged(bool isMuted)
    {
        IsMuted = isMuted;
    }
}
