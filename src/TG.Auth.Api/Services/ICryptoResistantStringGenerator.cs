namespace TG.Auth.Api.Services
{
    public interface ICryptoResistantStringGenerator
    {
        string Generate(int length);
    }
}