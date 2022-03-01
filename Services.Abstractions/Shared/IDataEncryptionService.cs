namespace Services.Abstractions.Shared
{
    public interface IDataEncryptionService
    {
        string Encrypt(string purpose, string input);
        string Decrypt(string purpose, string input);
    }
}
