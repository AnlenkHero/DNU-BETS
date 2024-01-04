public static class UserData
{
    public static string Name;
    public static double Balance;
    public static string UserId;
    
    public static bool IsUserDataLoaded()
    {
        return !string.IsNullOrEmpty(UserData.UserId);
    }

}