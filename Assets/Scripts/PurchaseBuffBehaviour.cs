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
            moneyView.Balance-=BuffPrice;
            UserRepository.UpdateUserBalance(UserData.UserId, UserData.Balance)
                .Then(_ => Debug.Log("success buff purchase"))
                .Catch(exception => Debug.Log(exception.Message));
        }
    }
}
