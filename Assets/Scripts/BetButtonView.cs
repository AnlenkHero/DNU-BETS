using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BetButtonView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;

    public void SetData(Contestant contestant)
    {
        buttonText.text = $"{contestant.Name} {contestant.Coefficient}";
    }

}
