using System.Linq;
using Libs.Config;
using Libs.Helpers;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class ResetBehaviour : MonoBehaviour
{
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private Button resetButton;

    private void Awake()
    {
        resetButton.onClick.AddListener(CheckConditions);
    }

    private void CheckConditions()
    {
        BetsRepository.GetAllBets(UserData.UserId).Then(bets =>
        {
            var hasActiveBets = bets.Any(bet => bet.IsActive);
            if (moneyView.Balance < ConfigManager.Settings.DefaultBalance && !hasActiveBets)
            {
                ShowResetPanel();
            }
            else
            {
                InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                    $"You have active bets or your balance greater or equal 300<color={ColorHelper.LightGreenString}>$</color>");
            }
        }).Catch(_ =>
        {
            if (moneyView.Balance < ConfigManager.Settings.DefaultBalance)
                ShowResetPanel();
            else
            {
                InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                    $"Your balance greater than 300<color={ColorHelper.LightGreenString}>$</color>");
            }
        });
    }

    private void ShowResetPanel()
    {
        InfoPanelManager.ShowPanel(ColorHelper.PaleYellow, $"CONFIRM RESET BALANCE TO {ConfigManager.Settings.DefaultBalance}<color={ColorHelper.LightGreenString}>$</color>?",
            () =>
            {
                InfoPanelManager.Instance.AddButton("Reset Money", ResetMoney, ColorHelper.LightGreenString);
                InfoPanelManager.Instance.AddButton("Decline", () => InfoPanelManager.Instance.HidePanel(),
                    ColorHelper.HotPinkString);
            });
    }

    private void ResetMoney()
    {
        UserRepository.GetUserById(UserData.UserId).Then(user =>
        {
            user.balance = ConfigManager.Settings.DefaultBalance;
            UserRepository.UpdateUserInfo(user).Then(helper =>
                {
                    moneyView.Balance = ConfigManager.Settings.DefaultBalance;
                    InfoPanelManager.ShowPanel(ColorHelper.LightGreen,
                        "Success money reset");
                    Debug.Log("Success money reset");
                })
                .Catch(exception =>
                {
                    InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                        $"Failed to reset user balance. {exception.Message}");
                });
        }).Catch(exception =>
        {
            InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                $"Failed to get user by id. {exception.Message}");
        });
        InfoPanelManager.Instance.HidePanel();
    }
}