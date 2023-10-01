using System.Linq;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class ResetBehaviour : MonoBehaviour
{
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private GameObject resetPanel;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button declineButton;
    private const int BalanceToReset = 300;

    private void Awake()
    { 
        resetButton.onClick.AddListener(CheckConditions);
        confirmButton.onClick.AddListener(ResetMoney);
        declineButton.onClick.AddListener(Decline);
    }

    private void CheckConditions()
    {
        BetsRepository.GetAllBetsByUserId(UserData.UserId).Then(bets =>
        {
            var hasActiveBets = bets.Any(bet => bet.IsActive);
            if(moneyView.Balance<=BalanceToReset && !hasActiveBets)
                resetPanel.SetActive(true);
            //TODO ELSE INFO PANEL
        });
    }

    private void Decline()
    {
        resetPanel.SetActive(false);
    }
    private void ResetMoney()
    {
        moneyView.Balance=BalanceToReset;
        UserRepository.UpdateUserBalance(UserData.UserId, UserData.Balance)
            .Then(_ => Debug.Log("success money reset"))
            .Catch(exception => Debug.Log(exception.Message));
        resetPanel.SetActive(false);
    }
}
