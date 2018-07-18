using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExampleUserController : SpotifyUIBase
{
    [SerializeField]
    Text m_username;

    [SerializeField]
    Text m_displayName;

    [SerializeField]
    Text m_userId;

    [SerializeField]
    Text m_country;

    [SerializeField]
    Text m_birthday;

    [SerializeField]
    Text m_followersCount;

    [SerializeField]
    Text m_isPremium;

    [SerializeField]
    Image m_profilePicture;

    protected override void OnUserInformationLoaded(UserInfoLoaded e)
    {
        base.OnUserInformationLoaded(e);

        m_username.text = e.Info.Username;
        m_displayName.text = e.Info.DisplayName;
        m_country.text = e.Info.Country;
        m_birthday.text = e.Info.Birthdate.ToString("dd/MM/yyyy");
        m_userId.text = e.Info.UserID;
        m_followersCount.text = e.Info.Followers.ToString();
        m_isPremium.text = e.Info.IsPremium.ToString();
    }

    protected override void OnUserProfilePictureLoaded(Sprite s)
    {
        base.OnUserProfilePictureLoaded(s);

        m_profilePicture.sprite = s;
    }
}
