using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Match
{
    public string ImageUrl;
    public string MatchTitle;
    public List<Contestant> Contestants;
    public bool IsFinished;
}

[System.Serializable]
public class Contestant
{
    public string Name;
    public decimal Coefficient;
    public bool Winner;
}