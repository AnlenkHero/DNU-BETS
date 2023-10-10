using System.Globalization;
using Cysharp.Threading.Tasks;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private Image profileImageBorder;

    public async void SetData(User user,Color color)
    {
        nameText.text = user.userName;
        moneyText.text = $"{user.Balance.ToString(CultureInfo.InvariantCulture)}<color=#90EE90>$</color>";
        profileImageBorder.color = color;
        nameText.color = color;
        moneyText.color = color;
        profileImage.texture = await MapImage(user.imageUrl);
    }

    private async UniTask<Texture> MapImage(string imageUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch image data: " + www.error);
            return null;
        }

        Texture2D texture2D = DownloadHandlerTexture.GetContent(www);
        return texture2D;
    }
}