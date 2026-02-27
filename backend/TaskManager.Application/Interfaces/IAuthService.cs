using System.Threading;
using System.Threading.Tasks;
using TaskManager.Application.DTOs.Auth;

namespace TaskManager.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    }
}
