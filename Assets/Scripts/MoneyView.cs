using System;
using System.Collections;
using Libs.Models;
using Libs.Repositories;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    private double _tempBalance;

    public double Balance
    {
        get => UserData.Balance;
        private set
        {
            _tempBalance = value; 
            Debug.Log(UserData.UserId);
            UserRepository.UpdateUserBalance(UserData.UserId, _tempBalance).Then(_ =>
            {
                UserData.Balance = _tempBalance;
                UpdateMoneyText();
            });
        }
    }
    
    private void UpdateMoneyText()
    {
        StopCoroutine(nameof(UpdateMoneyTextAsync));
        StartCoroutine(UpdateMoneyTextAsync(UserData.Balance));
    }

    public void AddMoney(double amount)
    {
        Balance += amount;
    }

    public void SubtractMoney(double amount)
    {
        Balance -= amount;
    }

    public void SetMoney(double amount)
    {
        Balance = amount;
    }

    private IEnumerator UpdateMoneyTextAsync(double targetAmount)
    {
        double currentAmount = double.Parse(moneyText.text);
        double step = (targetAmount - currentAmount) / 10; 

        for (int i = 0; i < 10; i++)
        {
            currentAmount += step;
            moneyText.text = Math.Round(currentAmount, 2).ToString();
            yield return new WaitForSeconds(0.05f); 
        }

        moneyText.text = Math.Round(targetAmount, 2).ToString();
    }
    
}