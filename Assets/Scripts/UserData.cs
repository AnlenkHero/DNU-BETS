using UnityEngine;

public static class UserData
{
    public static string Name;
    public static double Balance;
    public static string UserId;
    public static Texture2D ProfileImage;
    public static bool IsUserDataLoaded()
    {
        return !string.IsNullOrEmpty(UserId);
    }
    public static void ClearUserData()
    {
        Name = string.Empty;
        Balance = 0;
        UserId = string.Empty;
        ProfileImage = null;
    }

}