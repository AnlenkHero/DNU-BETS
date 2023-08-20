using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BetsHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField tmpInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private MoneyView money;

    public delegate void BetSubmittedHandler(decimal betAmount);
    public event BetSubmittedHandler OnBetSubmitted;

    private void Awake()
    {
        submitButton.onClick.AddListener(SubmitBet);
    }

    private void SubmitBet()
    {
        if (decimal.TryParse(tmpInputField.text, out decimal parsedValue) && parsedValue<= money.Balance)
        {
            OnBetSubmitted?.Invoke(parsedValue);
            gameObject.SetActive(false);
        }
        else
        {
            tmpInputField.text = "Invalid input for bet amount.";
            Debug.LogError("Invalid input for bet amount.");
        }
    }

    public void InitializeBetMenu()
    {
        gameObject.SetActive(true);
    }
}
