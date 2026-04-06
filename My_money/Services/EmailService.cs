using My_money.Constants;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace My_money.Services
{
    public static class EmailService
    {
        public static async Task SendAsync(string to, string subject, string body)
        {
            using var smtpClient = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(Secrets.Email, Secrets.Password),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(Secrets.Email),
                Subject = subject,
                Body = body
            };

            message.To.Add(to);

            try
            {
                await smtpClient.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending email", ex);
            }
        }
    }
}
