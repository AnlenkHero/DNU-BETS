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
        if(moneyView.Balance<=BalanceToReset)
            ResetMoney();
        else
        {
            resetPanel.SetActive(true);
        }
    }

    private void Decline()
    {
        resetPanel.SetActive(false);
    }
    private void ResetMoney()
    {
        moneyView.SetMoney(BalanceToReset);
        resetPanel.SetActive(false);
    }
}
