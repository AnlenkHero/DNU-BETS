using System.Collections.Generic;

[System.Serializable]
public class Match
{
    public string ImageUrl;
    public string MatchTitle;
    public List<Contestant> Contestants;
    public bool IsFinished;
}