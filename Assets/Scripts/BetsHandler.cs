using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class BetsHandler : MonoBehaviour
{
    [SerializeField] private TMP_InputField tmpInputField;
    [SerializeField] private Button submitButton;

    public delegate void BetSubmittedHandler(decimal betAmount);
    public event BetSubmittedHandler OnBetSubmitted;

    private void Awake()
    {
        submitButton.onClick.AddListener(SubmitBet);
    }

    private void SubmitBet()
    {
        if (decimal.TryParse(tmpInputField.text, out decimal parsedValue))
        {
            OnBetSubmitted?.Invoke(parsedValue);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("Invalid input for bet amount.");
        }
    }

    public void InitializeBetMenu()
    {
        gameObject.SetActive(true);
    }
}
