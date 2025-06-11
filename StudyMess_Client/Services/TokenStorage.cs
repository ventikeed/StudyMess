namespace StudyMess_Client.Services
{
    public static class TokenStorage
    {
        public static string? Token
        {
            get => Properties.Settings.Default.AuthToken;
        }

        public static void SetToken(string token)
        {
            Properties.Settings.Default.AuthToken = token;
            Properties.Settings.Default.Save();
        }

        public static void Clear()
        {
            Properties.Settings.Default.AuthToken = null;
            Properties.Settings.Default.Save();
        }

        public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    }
}
