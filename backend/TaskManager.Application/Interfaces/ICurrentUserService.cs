using System;

namespace TaskManager.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Guid GetCurrentUserId();
        string GetCurrentUserName();
        string GetCurrentUserEmail();
    }
}
