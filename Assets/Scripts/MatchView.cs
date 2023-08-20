using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MatchView : MonoBehaviour
{
    [SerializeField] private RawImage matchImage;
    [SerializeField] private TextMeshProUGUI title;
    public Transform buttonParent;
    [SerializeField] private BetButtonView buttonPrefab;
    public List<BetButtonView> buttonViews;
    public Match Match { get; private set; }
    public async void SetData(Match match)
    {
        Match = match;
        matchImage.texture = await MapImage(match.ImageUrl);
        title.text = match.MatchTitle;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(match.ImageUrl);
        buttonViews = new List<BetButtonView>();
        foreach (var contender in match.Contestants)
        {
           var button = Instantiate(buttonPrefab, buttonParent);
           button.SetData(contender);
           buttonViews.Add(button);
        }
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
