using System;
using Libs.Helpers;
using Libs.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetButtonEventArgs : EventArgs
{
    public Contestant Contestant { get; set; }
    public string MatchId { get; set; }

    public MatchView MatchViewParent;

    public BetButtonEventArgs(Contestant contestant, string matchId, MatchView matchView)
    {
        Contestant = contestant;
        MatchId = matchId;
        MatchViewParent = matchView;
    }
}

public class BetButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button betButton;

    public Contestant Contestant { get; private set; }

    public delegate void BetButtonEventHandler(object sender, BetButtonEventArgs args);

    public static event BetButtonEventHandler OnButtonClick;

    public void SetData(Contestant contestant, string matchId, MatchView matchView)
    {
        buttonText.text = $"{contestant.Name} \n<color={ColorHelper.LightGreenString}>{contestant.Coefficient}</color>";
        Contestant = contestant;
        betButton.onClick.AddListener(() => RaiseEvent(contestant, matchId, matchView));
    }

    private void RaiseEvent(Contestant contestant, string matchId, MatchView matchView)
    {
        OnButtonClick?.Invoke(this, new BetButtonEventArgs(contestant, matchId, matchView));
    }

    public void Hide()
    {
        betButton.interactable = false;
    }

    public void Show()
    {
        betButton.interactable = true;
    }
}