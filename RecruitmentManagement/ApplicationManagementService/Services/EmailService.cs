using System.Net;
using System.Net.Mail;

namespace ApplicationManagementService.Services;

public class EmailService : IEmailService
{
    public async Task SendEmailWithAttachmentAsync(string to, string subject, string body, string attachmentPath)
    {
        try
        {
            using var client = new SmtpClient("smtp.server.com");
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("username", "password");

            var mailMessage = new MailMessage
            {
                From = new MailAddress("sender@example.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);
            mailMessage.Attachments.Add(new Attachment(attachmentPath));
            await client.SendMailAsync(mailMessage);
        
            Console.WriteLine("Email sent successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send email. Exception: {ex.Message}");
        }
    }
}