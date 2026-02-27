using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ILogger<AuthService> _logger;
        private readonly AuthSettings _authSettings;

        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtTokenGenerator jwtTokenGenerator,
            IOptions<AuthSettings> authSettings,
            ILogger<AuthService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
            _authSettings = authSettings.Value;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            var existing = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
            if (existing != null)
            {
                throw new InvalidOperationException("User with the given email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                PasswordHash = HashPassword(request.Password),
                CreatedOn = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered with email {Email}", user.Email);

            var tokenResult = _jwtTokenGenerator.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var requestHash = HashPassword(request.Password);
            if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(requestHash),
                Encoding.UTF8.GetBytes(user.PasswordHash)))
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }

            var tokenResult = _jwtTokenGenerator.GenerateToken(user);

            _logger.LogInformation("User logged in with email {Email}", user.Email);

            return new AuthResponseDto
            {
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password + _authSettings.PasswordSalt);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    public class AuthSettings
    {
        public string PasswordSalt { get; set; } = string.Empty;
    }

    public interface IJwtTokenGenerator
    {
        JwtTokenResult GenerateToken(User user);
    }

    public class JwtTokenResult
    {
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }
    }
}
