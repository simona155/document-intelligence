using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DocumentIntelligence.Application.DTOs.Auth;
using DocumentIntelligence.Application.Interfaces;
using DocumentIntelligence.Domain.Entities;
using DocumentIntelligence.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DocumentIntelligence.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        if (user is null ||
            !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        var issuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            UserId = user.Id,
            Email = user.Email
        };
    }
}