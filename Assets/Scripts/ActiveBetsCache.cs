using System.Collections.Generic;
using UnityEngine;

public static class ActiveBetsCache
{
    public static void AddActiveBetId(string betId)
    {
        string existingBets = PlayerPrefs.GetString("ActiveBets", "");
        
        if (!string.IsNullOrEmpty(existingBets))
        {
            existingBets += ",";
        }
        existingBets += betId;
        
        PlayerPrefs.SetString("ActiveBets", existingBets);
        PlayerPrefs.Save();
    }
    public static List<string> GetAllActiveBetIds()
    {
        string betIdsString = PlayerPrefs.GetString("ActiveBets", "");
        
        if (string.IsNullOrEmpty(betIdsString))
        {
            return new List<string>();
        }
        
        string[] betIds = betIdsString.Split(',');
        return new List<string>(betIds);
    }
    public static void RemoveActiveBetId(string betId)
    {
        List<string> betIds = GetAllActiveBetIds();
        
        if (betIds.Remove(betId))
        {
            string updatedBetIdsString = string.Join(",", betIds);
            PlayerPrefs.SetString("ActiveBets", updatedBetIdsString);
            PlayerPrefs.Save();
        }
    }

}