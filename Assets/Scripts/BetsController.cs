using System.Collections;
using UnityEngine;

public class BetsController : MonoBehaviour
{
    [SerializeField] private BetsHandler betsHandler;
    [SerializeField] private MoneyView moneyView;
    private BetButtonEventArgs _betButtonEventArgs;
    
    private void OnEnable()
    {
        betsHandler.OnBetSubmitted += HandleBetSubmitted;
        BetButtonView.OnButtonClick += InitializeBetFields;
    }

    private void OnDisable()
    {
        betsHandler.OnBetSubmitted -= HandleBetSubmitted;
        BetButtonView.OnButtonClick -= InitializeBetFields;
    }

    private void HandleBetSubmitted(double betAmount)
    {
        foreach (var selectedMatchButton in SwipeMenu.SelectedMatchButtons)
        {
            selectedMatchButton.Hide();
        }
    
        moneyView.SubtractMoney(betAmount);
        Debug.Log(moneyView.Balance);
        StartCoroutine(WaitAndLog(_betButtonEventArgs));
    }


    private void InitializeBetFields(object sender, BetButtonEventArgs args)
    {
        _betButtonEventArgs = args;
        betsHandler.InitializeBetMenu();
    }

    private IEnumerator WaitAndLog(BetButtonEventArgs args)
    {
        yield return new WaitForSeconds(2);
        Debug.Log(moneyView.Balance);
    }
}