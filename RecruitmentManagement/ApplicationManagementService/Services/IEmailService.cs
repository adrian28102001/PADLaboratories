namespace ApplicationManagementService.Services;

public interface IEmailService
{
    Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath);
}