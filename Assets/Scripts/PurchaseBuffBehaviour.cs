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
            moneyView.SubtractMoney(BuffPrice);
        }
    }
}
