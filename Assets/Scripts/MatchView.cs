using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Search;
using UnityEngine.UI;

public class MatchView : MonoBehaviour
{
    [SerializeField] private RawImage matchImage;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private GameObject buttonPrefab;

    public void SetData(Match match)
    {
        title.text = match.MatchTitle;
        
    }
}
