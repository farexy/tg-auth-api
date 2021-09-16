using System.Threading.Tasks;

namespace TG.Auth.Api.Services
{
    public interface ILoginGenerator
    {
        Task<string> GenerateLoginAsync(string? email);
    }
}