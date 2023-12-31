﻿using System.Net;
using System.Net.Mail;
using ApplicationManagementService.Models;
using Microsoft.Extensions.Options;

namespace ApplicationManagementService.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }
    
    public async Task SendEmailWithAttachmentAsync(string to, string body, string attachmentPath)
         {
             try
             {
                 using var client = new SmtpClient("smtp.gmail.com")
                 {
                     Port = 587,
                     EnableSsl = true,
                     UseDefaultCredentials = false,
                     Credentials = new NetworkCredential(_emailSettings.User, _emailSettings.Password)
                 };
     
                 var mailMessage = new MailMessage
                 {
                     To = { new MailAddress(_emailSettings.Recipient) },
                     From = new MailAddress(_emailSettings.User),
                     Subject = _emailSettings.Subject,
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