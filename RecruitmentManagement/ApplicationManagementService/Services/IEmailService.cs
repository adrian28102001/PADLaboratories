namespace ApplicationManagementService.Services;

public interface IEmailService
{
    Task SendEmailWithAttachmentAsync(string to, string body, string attachmentPath);
}