using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MoneyView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    private double currentAmount;

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
        double step = (targetAmount - currentAmount) / 10; 

        for (int i = 0; i < 10; i++)
        {
            currentAmount += step;
            moneyText.text = $"{Math.Round(currentAmount, 2).ToString()}$";
            yield return new WaitForSeconds(0.05f); 
        }

        currentAmount = targetAmount;
        moneyText.text = $"{Math.Round(targetAmount, 2).ToString()}$";
    }

    
}