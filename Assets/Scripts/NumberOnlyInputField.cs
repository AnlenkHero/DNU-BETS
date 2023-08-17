using TMPro;
using UnityEngine;

public class NumberOnlyInputField : MonoBehaviour
{
    private TMP_InputField _inputField;

    private void Awake()
    {
        _inputField = GetComponent<TMP_InputField>();
        _inputField.onValidateInput += ValidateInput;
        _inputField.onSelect.AddListener(ResetPlaceholder);
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (char.IsDigit(addedChar))
        {
            return addedChar;
        }
        return '\0';
    }

    private void ResetPlaceholder(string value)
    {
        _inputField.placeholder.gameObject.SetActive(false);
    }
}