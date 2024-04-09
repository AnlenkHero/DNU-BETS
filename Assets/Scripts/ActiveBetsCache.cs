using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ActiveBetsCache
{
    private const string ActiveBetsKey = "ActiveBets";
    private const char BetIdsDelimiter = ',';
    
    public static void AddActiveBetId(int betId)
    {
        string existingBets = PlayerPrefs.GetString(ActiveBetsKey, string.Empty);
        
        if (!string.IsNullOrEmpty(existingBets))
        {
            existingBets += BetIdsDelimiter;
        }
        existingBets += betId;
        
        PlayerPrefs.SetString("ActiveBets", existingBets);
        PlayerPrefs.Save();
    }
    public static IEnumerable<int> GetAllActiveBetIds()
    {
        string betIdsString = PlayerPrefs.GetString(ActiveBetsKey, string.Empty); 
        if (string.IsNullOrEmpty(betIdsString))
        {
            return new List<int>();
        }
        
        return betIdsString.Split(BetIdsDelimiter).Cast<int>();
    }
    
    public static void RemoveActiveBetId(int betId)
    {
        List<int> betIds = GetAllActiveBetIds().ToList();

        if (betIds.Remove(betId))
        {
            string updatedBetIdsString = string.Join(BetIdsDelimiter, betIds);
            PlayerPrefs.SetString(ActiveBetsKey, updatedBetIdsString);
            PlayerPrefs.Save();
        }
    }

}