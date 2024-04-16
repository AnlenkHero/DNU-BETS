using System;
using System.Globalization;
using Libs.Config;
using Libs.Helpers;
using Libs.Models;
using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseBuffBehaviour : MonoBehaviour
{
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private Button purchaseBuffButton;

    private void Awake()
    {
        purchaseBuffButton.onClick.AddListener(ShowPurchaseBuffPanel);
    }

    private void ShowPurchaseBuffPanel()
    {
        InfoPanelManager.ShowPanel(ColorHelper.PaleYellow, "CONFIRM BUFF PURCHASE?",
                () =>
                {
                    InfoPanelManager.Instance.AddButton("Purchase Buff", PurchaseBuff, ColorHelper.LightGreenString);
                    InfoPanelManager.Instance.AddButton("Decline", () => InfoPanelManager.Instance.HidePanel(), ColorHelper.HotPinkString);
                });
    }
    
    private void PurchaseBuff()
    {
        var buffPurchase = new BuffPurchase()
        {
            quantity = 1,
            userId = UserData.UserId
        };

        BuffPurchasesRepository.CreatePurchase(buffPurchase).Then(id =>
        {
            InfoPanelManager.ShowPanel(ColorHelper.LightGreen,
                "Buff is purchased successfully!");
            
            UserRepository.GetUserBalanceById(UserData.UserId)
                .Then(balance => { moneyView.Balance = balance; })
                .Catch(Debug.LogError);
        }).Catch(e =>
        {
            InfoPanelManager.ShowPanel(ColorHelper.HotPink,
                $"Failed to purchase buff: {e.Message}");        
        });

    }
}