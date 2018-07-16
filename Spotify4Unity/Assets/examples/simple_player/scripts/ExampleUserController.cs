using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleUserController : SpotifyUIBase
{
    [SerializeField]
    Text m_username;

    [SerializeField]
    Image m_profilePicture;

    protected override void OnUserInformationLoaded(UserInfoLoaded e)
    {
        base.OnUserInformationLoaded(e);

        m_username.text = e.Info.Username;
    }

    protected override void OnUserProfilePictureLoaded(Sprite s)
    {
        base.OnUserProfilePictureLoaded(s);

        m_profilePicture.sprite = s;
    }
}
