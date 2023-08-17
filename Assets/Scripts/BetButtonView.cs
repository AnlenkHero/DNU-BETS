using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BetButtonEventArgs : EventArgs
{
    public Contestant Contestant { get; set; }
    public BetButtonEventArgs(Contestant contestant)
    {
        this.Contestant = contestant;
    }
}
public class BetButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button betButton;
    public Contestant Contestant { get; private set; }
    public delegate void BetButtonEventHandler(object sender, BetButtonEventArgs args);

    public static event BetButtonEventHandler OnButtonClick;
    public void SetData(Contestant contestant)
    {
        buttonText.text = $"{contestant.Name} {contestant.Coefficient}";
        Contestant = contestant;
        betButton.onClick.AddListener(() => RaiseEvent(contestant));
    }

    private void RaiseEvent(Contestant contestant)
    {
        OnButtonClick?.Invoke(this,new BetButtonEventArgs(contestant));
    }

    public void Hide()
    {
        betButton.interactable = false;
    }

}
