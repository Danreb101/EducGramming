using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EducGramming.Services
{
    public class EmailService
    {
        private readonly string smtpHost = "smtp.gmail.com"; // Or your SMTP server
        private readonly int smtpPort = 587;
        private readonly string smtpUser = "your-email@gmail.com"; // TODO: Replace with your sender email
        private readonly string smtpPass = "your-app-password"; // TODO: Replace with your app password

        public async Task SendPasswordChangedEmailAsync(string toEmail, string userName)
        {
            var message = new MailMessage();
            message.From = new MailAddress(smtpUser, "EducGramming");
            message.To.Add(toEmail);
            message.Subject = "Your password was changed";
            message.Body = $"Hello {userName},\n\nYour password for EducGramming was recently changed. If you did not make this change, please reset your password immediately or contact support.\n\nThanks,\nThe EducGramming Team";

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);
                await client.SendMailAsync(message);
            }
        }
    }
} 