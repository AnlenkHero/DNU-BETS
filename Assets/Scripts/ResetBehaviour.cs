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
            if (moneyView.Balance <= BalanceToReset && !hasActiveBets)
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
        UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
        {
            user.Balance = BalanceToReset;
            UserRepository.UpdateUserBalance(user).Then(helper =>
                {
                    moneyView.Balance = BalanceToReset;
                    Debug.Log("Success money reset");
                })
                .Catch(exception => Debug.Log($"Failed to reset user balance {exception.Message}"));
        }).Catch(exception => { Debug.Log($"Failed to get user by id {exception.Message}"); });
        resetPanel.SetActive(false);
    }
}