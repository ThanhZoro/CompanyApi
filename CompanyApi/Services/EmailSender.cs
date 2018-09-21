using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CompanyApi.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailSender : IEmailSender
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sendGridUser"></param>
        /// <param name="sendGridKey"></param>
        public EmailSender(string sendGridUser, string sendGridKey)
        {
            //Options.SendGridUser = sendGridUser;
            SendGridKey = sendGridKey;
        }

        /// <summary>
        /// 
        /// </summary>
        public AuthMessageSenderOptions Options { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SendGridKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(SendGridKey, subject, message, email);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("noreply@email-quarantine.google.com", "TNT MGMT"),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));
            return client.SendEmailAsync(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendEmailAsync(string apiKey, SendGridMessage message)
        {
            var client = new SendGridClient(apiKey);
            return client.SendEmailAsync(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public Task SendSmtpEmailAsync(Contracts.Models.MailSettings settings, MailMessage message)
        {
            SmtpClient SmtpServer = new SmtpClient(settings.Host, 587);
            SmtpServer.Credentials = new System.Net.NetworkCredential(settings.Email, settings.Password);
            SmtpServer.EnableSsl = settings.EnableSSL;
            return SmtpServer.SendMailAsync(message);
        }
    }
}
