using Application.DTOs;

namespace Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendAsync(EmailRequest request);
    }
}