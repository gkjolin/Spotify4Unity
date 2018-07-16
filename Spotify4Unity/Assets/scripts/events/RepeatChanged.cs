public class RepeatChanged : GameEventBase
{
    public Repeat State { get; set; }
    public RepeatChanged(Repeat state)
    {
        State = state;
    }
}
