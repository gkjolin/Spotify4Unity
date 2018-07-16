using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo
{
    /// <summary>
    /// The username of the
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The display name of the user. Usually first and last name
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Is the user a premium user
    /// </summary>
    public bool IsPremium { get; set; }
    /// <summary>
    /// How many followers does the user has
    /// </summary>
    public int Followers { get; set; }

    /// <summary>
    /// The ID of the user
    /// </summary>
    public string UserID { get; set; }
    /// <summary>
    /// The URl of the first profile picture for the user, can be empty
    /// </summary>
    public string ProfilePictureURL { get; set; }
    /// <summary>
    /// A code of the country for the user. For example, if the user is in Great Britain, it will be "GB"
    /// </summary>
    public string Country { get; set; }
    /// <summary>
    /// The birthdate of the user. Will be DateTime.Min if invalid or unavailable
    /// </summary>
    public DateTime Birthdate { get; set; }
}
