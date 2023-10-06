using Libs.Repositories;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseBuffBehaviour : MonoBehaviour
{
    private const int BuffPrice = 10000;
    [SerializeField] private MoneyView moneyView;
    [SerializeField] private Button purchaseBuffButton;

    private void Awake()
    {
        purchaseBuffButton.onClick.AddListener(PurchaseBuff);
    }

    private void PurchaseBuff()
    {
        if (moneyView.Balance >= BuffPrice)
        {
            var tempBalance = moneyView.Balance - BuffPrice;
            
            UserRepository.GetUserByUserId(UserData.UserId).Then(user =>
            {
                user.Balance = tempBalance;
                UserRepository.UpdateUserBalance(user).Then(helper =>
                    {
                        moneyView.Balance -= BuffPrice;
                        Debug.Log("Success buff purchase");
                    })
                    .Catch(exception => Debug.Log($"Failed to reset user balance {exception.Message}"));
            }).Catch(exception => { Debug.Log($"Failed to get user by id {exception.Message}"); });
        }
    }
}