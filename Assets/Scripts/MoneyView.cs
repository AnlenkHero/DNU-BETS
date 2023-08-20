using System;
using System.Collections;
using Libs.Models;
using TMPro;
using UnityEngine;

public class MoneyView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    public decimal Balance
    {
        get => UserData.Balance;
        private set
        {
            UserData.Balance = value;
            UpdateMoneyText();
        }
    }
    
    private void UpdateMoneyText()
    {
        StopCoroutine(nameof(UpdateMoneyTextAsync));
        StartCoroutine(UpdateMoneyTextAsync(UserData.Balance));
    }

    public void AddMoney(decimal amount)
    {
        Balance += amount;
    }

    public void SubtractMoney(decimal amount)
    {
        Balance -= amount;
    }

    public void SetMoney(decimal amount)
    {
        Balance = amount;
    }

    private IEnumerator UpdateMoneyTextAsync(decimal targetAmount)
    {
        decimal currentAmount = decimal.Parse(moneyText.text);
        decimal step = (targetAmount - currentAmount) / 10; 

        for (int i = 0; i < 10; i++)
        {
            currentAmount += step;
            moneyText.text = Math.Round(currentAmount, 2).ToString();
            yield return new WaitForSeconds(0.05f); 
        }

        moneyText.text = Math.Round(targetAmount, 2).ToString();
    }
}