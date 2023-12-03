using System;
using System.Globalization;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseBuffBehaviour : MonoBehaviour
{
    private int _buffPrice;
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private Button purchaseBuffButton;

    private void Awake()
    {
        purchaseBuffButton.onClick.AddListener(PurchaseBuff);
    }

    private void PurchaseBuff()
    {
        AppSettingsRepository.GetAppSettings().Then(settings =>
        {
            _buffPrice = settings.buffPrice;
            if (moneyView.Balance >= _buffPrice)
            {
                var tempBalance = moneyView.Balance - _buffPrice;

                UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
                {
                    user.balance = tempBalance;
                    BuffPurchase buffPurchase = new BuffPurchase();
                    buffPurchase.date = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
                    buffPurchase.price = _buffPrice;
                    buffPurchase.quantity = 1;
                    buffPurchase.isProcessed = false;

                    user.buffPurchase.Add(buffPurchase);
                    
                    UserRepository.UpdateUserInfo(user).Then(helper =>
                        {
                            moneyView.Balance -= _buffPrice;
                            InfoPanel.ShowPanel(ColorHelper.LightGreen,
                                "Success buff purchase");
                            Debug.Log("Success buff purchase");
                        })
                        .Catch(exception =>
                        {
                            InfoPanel.ShowPanel(ColorHelper.HotPink,
                                $"Failed to purchase buff. {exception.Message}");
                        });
                }).Catch(exception =>
                {
                    InfoPanel.ShowPanel(ColorHelper.HotPink,
                        $"Failed to get user by id. {exception.Message}");
                });
            }
            else
            {
                InfoPanel.ShowPanel(ColorHelper.HotPink,
                    $"You dont have enough money. Buff price is <color={ColorHelper.LightGreenString}>{_buffPrice}$</color>");
            }
        }).Catch(exception =>
        {
            InfoPanel.ShowPanel(ColorHelper.HotPink,
                $"Failed to get buff price.\n{exception.Message}");
        });
    }
}