using System.Globalization;
using Libs.Helpers;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Image profileImageBorder;

    public void SetData(User user,Color color)
    {
        nameText.text = user.userName;
        moneyText.text = $"{user.balance.ToString(CultureInfo.InvariantCulture)}<color=#90EE90>$</color>";
        profileImageBorder.color = color;
        nameText.color = color;
        moneyText.color = color;
        TextureLoader.LoadTexture(this, user.imageUrl, texture2D =>
        {
            if (texture2D != null)
            {
                profileImage.texture = texture2D;
            }
            else
            {
                Debug.Log("Texture failed to load.");
            }
        });
    }
}