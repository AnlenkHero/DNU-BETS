using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BetsHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField tmpInputField;
    [SerializeField] private Button submitButton;

    public delegate void BetSubmittedHandler(double betAmount);
    public event BetSubmittedHandler OnBetSubmit;

    private void Awake()
    {
        submitButton.onClick.AddListener(SubmitBet);
    }

    private void SubmitBet()
    {
        if (double.TryParse(tmpInputField.text, out double parsedValue) && parsedValue <= UserData.Balance)
        {
            OnBetSubmit?.Invoke(parsedValue);
            ExitBetMenu();
        }
        else
        {
            tmpInputField.text = "Invalid bet.";
            Debug.LogError($"Invalid bet. Bet amount {parsedValue} user balance {UserData.Balance}");
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
