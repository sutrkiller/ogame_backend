namespace OGame.Configuration.Contracts.Settings
{
    public class TokenSettings
    {
        public string Key { get; set; }

        public string Issuer { get; set; }

        public int ExpirationDays { get; set; }
    }
}
