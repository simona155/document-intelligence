using DocumentIntelligence.Application.DTOs.Auth;

namespace DocumentIntelligence.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    Task<AuthResponse> LoginAsync(LoginRequest request);
}