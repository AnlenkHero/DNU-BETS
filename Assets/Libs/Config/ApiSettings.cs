using System;

namespace Libs.Config
{
    [Serializable]
    public class ApiSettings
    {
        public string Url;
        public string Login;
        public string Password;
        public string LoginEnpoint;
        public int TokenLifeTimeInSeconds;

        public bool UseAuthentication()
        {
            return !string.IsNullOrWhiteSpace(Login) 
                   && !string.IsNullOrWhiteSpace(Password) 
                   && !string.IsNullOrWhiteSpace(LoginEnpoint);
        }
    }
}