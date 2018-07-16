using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Repeat
{
    /// <summary>
    /// Spotify won't repeat any songs
    /// </summary>
    Disabled = 0,
    /// <summary>
    /// Will repeat the current playlist
    /// </summary>
    Playlist = 1,
    /// <summary>
    /// Repeats the current track
    /// </summary>
    Track = 2,
}