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


    public double Balance
    {
        get => UserData.Balance; 
        set
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