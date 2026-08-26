namespace ImmatureBackend.Application.Interfaces;

public interface IAuthService
{
    bool IsValidKey(string key);
}