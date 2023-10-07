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
            if (moneyView.Balance < BalanceToReset && !hasActiveBets)
                resetPanel.SetActive(true);
            else
            {
                InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                    "You have active bets or your balance greater or equal than 300$");
            }
        }).Catch(_ =>
        {
            if (moneyView.Balance < BalanceToReset)
                resetPanel.SetActive(true);
            else
            {
                InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                    "Your balance greater than 300$");
            }
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
                    InfoPanel.ShowPanel(new Color32(0x2F, 0xFF, 0x2F, 0xFF),
                        "Success money reset");
                    Debug.Log("Success money reset");
                })
                .Catch(exception =>
                {
                    InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                        $"Failed to reset user balance. {exception.Message}");
                });
        }).Catch(exception =>
        {
            InfoPanel.ShowPanel(new Color32(0xFF, 0x44, 0x91, 0xFF),
                $"Failed to get user by id. {exception.Message}");
        });
        resetPanel.SetActive(false);
    }
}