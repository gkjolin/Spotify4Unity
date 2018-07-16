public class ShuffleChanged : GameEventBase
{
    public Shuffle State { get; set; }
    public ShuffleChanged(Shuffle state)
    {
        State = state;
    }
}