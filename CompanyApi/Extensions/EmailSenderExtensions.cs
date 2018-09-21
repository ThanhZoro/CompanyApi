using Contracts.Commands;
using Contracts.Models;
using CompanyApi.Models;
using CompanyApi.Services;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class EmailSenderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailSender"></param>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <param name="userId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static async Task<EmailLog> SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string code, string userId, string culture)
        {
            string body = string.Empty;
            string subject = string.Empty;
            string link = string.Empty;
            switch (culture)
            {
                case "vi-VN":
                    link = "./wwwroot/EmailTemplates/ActiveAccount-VN.html";
                    subject = "Mã kích hoạt tài khoản";
                    break;
                case "en-US":
                    link = "./wwwroot/EmailTemplates/ActiveAccount-US.html";
                    subject = "TwinCRM account activation code";
                    break;
                default:
                    link = "./wwwroot/EmailTemplates/ActiveAccount-VN.html";
                    subject = "Mã kích hoạt tài khoản";
                    break;
            }
            using (StreamReader reader = File.OpenText(link))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{OTPNumber}", code);
            await emailSender.SendEmailAsync(email, subject, body);
            return new EmailLog()
            {
                ObjectId = userId,
                ObjectType = "users",
                Sender = "noreply@email-quarantine.google.com",
                Receiver = email,
                Body = body,
                Subject = subject,
                SendTime = DateTime.UtcNow,
                Type = TypeNotification.VerifyAccount.ToString()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailSender"></param>
        /// <param name="email"></param>
        /// <param name="callbackUrl"></param>
        /// <param name="userId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static async Task<EmailLog> SendEmailForgotPasswordAsync(this IEmailSender emailSender, string email, string callbackUrl, string userId, string culture)
        {
            string body = string.Empty;
            string link = string.Empty;
            string subject = string.Empty;
            switch (culture)
            {
                case "vi-VN":
                    link = "./wwwroot/EmailTemplates/ForgotPassword-VN.html";
                    subject = "Đặt lại mật khấu";
                    break;
                case "en-US":
                    link = "./wwwroot/EmailTemplates/ForgotPassword-US.html";
                    subject = "Account reset your password";
                    break;
                default:
                    link = "./wwwroot/EmailTemplates/ForgotPassword-VN.html";
                    subject = "Đặt lại mật khấu";
                    break;
            }
            using (StreamReader reader = File.OpenText(link))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{callbackUrl}", callbackUrl);
            await emailSender.SendEmailAsync(email, subject, body);
            return new EmailLog()
            {
                ObjectId = userId,
                ObjectType = "users",
                Sender = "noreply@email-quarantine.google.com",
                Receiver = email,
                Body = body,
                Subject = subject,
                SendTime = DateTime.UtcNow,
                Type = TypeNotification.ForgotPassword.ToString()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailSender"></param>
        /// <param name="email"></param>
        /// <param name="user"></param>
        /// <param name="userId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static async Task<EmailLog> SendEmailRegistrationAsync(this IEmailSender emailSender, string email, RegisterAccount user, string userId, string culture)
        {
            string body = string.Empty;
            string link = string.Empty;
            string subject = string.Empty;
            switch (culture)
            {
                case "vi-VN":
                    link = "./wwwroot/EmailTemplates/RegisterAccount-VN.html";
                    subject = "Chúc mừng bạn đã đăng ký tài khoản thành công";
                    break;
                case "en-US":
                    link = "./wwwroot/EmailTemplates/RegisterAccount-US.html";
                    subject = "Congratulations for successful registration account";
                    break;
                default:
                    link = "./wwwroot/EmailTemplates/RegisterAccount-VN.html";
                    subject = "Chúc mừng bạn đã đăng ký tài khoản thành công";
                    break;
            }
            using (StreamReader reader = File.OpenText(link))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{userName}", user.UserName);
            body = body.Replace("{password}", user.Password);
            body = body.Replace("{linkLogin}", user.LinkLogin);
            body = body.Replace("{fullName}", user.FullName);
            body = body.Replace("{companyName}", user.CompanyName);
            body = body.Replace("{positions}", String.Join(", ", user.Position));
            body = body.Replace("{companyName}", user.CompanyName);
            body = body.Replace("{phoneNumber}", user.PhoneNumber);
            body = body.Replace("{email}", user.Email);
            await emailSender.SendEmailAsync(email, subject, body);
            return new EmailLog()
            {
                ObjectId = userId,
                ObjectType = "users",
                Sender = "noreply@email-quarantine.google.com",
                Receiver = email,
                Body = body,
                Subject = subject,
                SendTime = DateTime.UtcNow,
                Type = TypeNotification.ForgotPassword.ToString()
            };
        }
    }
}

