using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? principal?.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return Guid.Parse(userId);
        }

        public string GetCurrentUserName()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var name = principal?.FindFirstValue("name");

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return name;
        }

        public string GetCurrentUserEmail()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            var email = principal?.FindFirstValue(ClaimTypes.Email)
                        ?? principal?.FindFirstValue("email");

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            return email;
        }
    }
}

