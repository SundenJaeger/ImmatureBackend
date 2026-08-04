namespace ImmatureBackend.Services.Interfaces;

public interface IAuthService
{
    bool IsValidKey(string key);
}