using System.Collections.Generic;
using System.Linq;
using Libs.Helpers;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchView : MonoBehaviour
{
    [SerializeField] private RawImage matchImage;
    [SerializeField] private TextMeshProUGUI title;
    public Transform buttonParent;
    [SerializeField] private BetButtonView buttonPrefab;
    public List<BetButtonView> buttonViews;
    public Match Match { get; private set; }
    public  void SetData(Match match)
    {
        TextureLoader.LoadTexture(this, match.ImageUrl, texture2D =>
        {
            if (texture2D != null)
            {
                matchImage.texture = texture2D;
            }
            else
            {
                Debug.Log("Texture failed to load.");
            }
        });
        
        Match = match;
        title.text = match.MatchTitle;
        buttonViews = new List<BetButtonView>();
        
        bool hasBets = BetCache.Bets?.Any(x => x.MatchId == match.Id)==true;
        
        
        foreach (var contender in match.Contestants)
        {
            var button = Instantiate(buttonPrefab, buttonParent);
            button.SetData(contender, match.Id,this);
            
            if (hasBets)
            {
                button.Hide();
            }
            
            buttonViews.Add(button);
        }
    }
    
    public void HideAllButtons()
    {
        foreach (var betButtonView in buttonViews)
        {
            betButtonView.Hide();
        }
    }
    
    public void ShowAllButtons()
    {
        foreach (var betButtonView in buttonViews)
        {
            betButtonView.Show();
        }
    }
}
