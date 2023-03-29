namespace Contracts.Catalog.Response
{
    public class Token
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class TokenResponse : Token
    {
        public DateTime? AccessTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new List<string>();
    }
}