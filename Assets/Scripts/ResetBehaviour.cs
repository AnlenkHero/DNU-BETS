using System.Linq;
using Libs.Helpers;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class ResetBehaviour : MonoBehaviour
{
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private Button resetButton;

    private const int BalanceToReset = 300;

    private void Awake()
    {
        resetButton.onClick.AddListener(CheckConditions);
    }

    private void CheckConditions()
    {
        BetsRepository.GetAllBetsByUserId(UserData.UserId).Then(bets =>
        {
            var hasActiveBets = bets.Any(bet => bet.IsActive);
            if (moneyView.Balance < BalanceToReset && !hasActiveBets)
                ShowResetPanel();
            else
            {
                InfoPanel.ShowPanel(ColorHelper.HotPink,
                    $"You have active bets or your balance greater or equal than 300<color={ColorHelper.LightGreenString}>$</color>");
            }
        }).Catch(_ =>
        {
            if (moneyView.Balance < BalanceToReset)
                ShowResetPanel();
            else
            {
                InfoPanel.ShowPanel(ColorHelper.HotPink,
                    $"Your balance greater than 300<color={ColorHelper.LightGreenString}>$</color>");
            }
        });
    }

    private void ShowResetPanel()
    {
        InfoPanel.ShowPanel(ColorHelper.PaleYellow, "CONFIRM RESET BALANCE TO 300$?",
            () =>
            {
                InfoPanel.Instance.AddButton("Reset Money", ResetMoney, ColorHelper.LightGreenString);
                InfoPanel.Instance.AddButton("Decline", () => InfoPanel.Instance.HidePanel(), ColorHelper.HotPinkString);
            });
    }

    private void ResetMoney()
    {
        UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
        {
            user.balance = BalanceToReset;
            UserRepository.UpdateUserInfo(user).Then(helper =>
                {
                    moneyView.Balance = BalanceToReset;
                    InfoPanel.ShowPanel(ColorHelper.LightGreen,
                        "Success money reset");
                    Debug.Log("Success money reset");
                })
                .Catch(exception =>
                {
                    InfoPanel.ShowPanel(ColorHelper.HotPink,
                        $"Failed to reset user balance. {exception.Message}");
                });
        }).Catch(exception =>
        {
            InfoPanel.ShowPanel(ColorHelper.HotPink,
                $"Failed to get user by id. {exception.Message}");
        });
        InfoPanel.Instance.HidePanel();
    }
}