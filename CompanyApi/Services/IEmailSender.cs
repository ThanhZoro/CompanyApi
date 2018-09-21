using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace CompanyApi.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="subject"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendEmailAsync(string email, string subject, string message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendEmailAsync(string apiKey, SendGridMessage message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task SendSmtpEmailAsync(Contracts.Models.MailSettings settings, MailMessage message);
    }
}

