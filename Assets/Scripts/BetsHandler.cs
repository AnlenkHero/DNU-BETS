using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BetsHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField tmpInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private MoneyView money;

    public delegate void BetSubmittedHandler(double betAmount);
    public event BetSubmittedHandler OnBetSubmitted;

    private void Awake()
    {
        submitButton.onClick.AddListener(SubmitBet);
    }

    private void SubmitBet()
    {
        if (double.TryParse(tmpInputField.text, out double parsedValue) && parsedValue<= money.Balance)
        {
            OnBetSubmitted?.Invoke(parsedValue);
            ExitBetMenu();
        }
        else
        {
            tmpInputField.text = "Invalid input for bet amount.";
            Debug.LogError("Invalid input for bet amount.");
        }
    }

    public void ExitBetMenu()
    {
        gameObject.SetActive(false);
    }
    public void InitializeBetMenu()
    {
        gameObject.SetActive(true);
    }
}
