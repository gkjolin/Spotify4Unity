using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayStatusChanged : GameEventBase
{
    public bool IsPlaying { get; set; }
    public PlayStatusChanged(bool isPlaying)
    {
        IsPlaying = isPlaying;
    }
}
