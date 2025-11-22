namespace MoqDemo.Core.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body);
    Task<bool> SendWelcomeEmailAsync(string to, string userName);
    Task<bool> SendOrderConfirmationAsync(string to, int orderId, decimal totalAmount);
}
