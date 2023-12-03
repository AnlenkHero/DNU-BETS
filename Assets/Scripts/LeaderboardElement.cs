using System.Globalization;
using JetBrains.Annotations;
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
    public void SetData(User user, Color color, [CanBeNull] Gradient wobbleGradient = null)
    {
        nameText.text = user.userName;
        moneyText.text = $"{user.balance.ToString(CultureInfo.InvariantCulture)}<color={ColorHelper.LightGreenString}>$</color>";
        profileImageBorder.color = color;
        nameText.color = color;
        moneyText.color = color;

        if (wobbleGradient != null)
        {
            moneyText.gameObject.AddComponent<WordWobble>().rainbow = wobbleGradient;
        }

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