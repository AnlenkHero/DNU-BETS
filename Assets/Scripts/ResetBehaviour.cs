using System.Linq;
using Libs.Helpers;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class ResetBehaviour : MonoBehaviour
{
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private Button resetButton;

    private double _balanceToReset = 300;

    private void Awake()
    {
        resetButton.onClick.AddListener(CheckConditions);
        SetBalanceToReset();
    }

    private void SetBalanceToReset()
    {
        AppSettingsRepository.GetAppSettings()
            .Then(settings => _balanceToReset = settings.defaultBalance)
            .Catch(_ => _balanceToReset = 300);
    }

    private void CheckConditions()
    {
        BetsRepository.GetAllBetsByUserId(UserData.UserId).Then(bets =>
        {
            var hasActiveBets = bets.Any(bet => bet.IsActive);
            if (moneyView.Balance < _balanceToReset && !hasActiveBets)
                ShowResetPanel();
            else
            {
                InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                    $"You have active bets or your balance greater or equal 300<color={ColorHelper.LightGreenString}>$</color>");
            }
        }).Catch(_ =>
        {
            if (moneyView.Balance < _balanceToReset)
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
        InfoPanelManager.ShowPanel(ColorHelper.PaleYellow, $"CONFIRM RESET BALANCE TO {_balanceToReset}<color={ColorHelper.LightGreenString}>$</color>?",
            () =>
            {
                InfoPanelManager.Instance.AddButton("Reset Money", ResetMoney, ColorHelper.LightGreenString);
                InfoPanelManager.Instance.AddButton("Decline", () => InfoPanelManager.Instance.HidePanel(),
                    ColorHelper.HotPinkString);
            });
    }

    private void ResetMoney()
    {
        UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
        {
            user.balance = _balanceToReset;
            UserRepository.UpdateUserInfo(user).Then(helper =>
                {
                    moneyView.Balance = _balanceToReset;
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
    }
}