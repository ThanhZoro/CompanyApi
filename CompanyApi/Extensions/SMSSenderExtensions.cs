using Contracts.Commands;
using Contracts.Models;
using CompanyApi.Services;
using System;
using System.Threading.Tasks;

namespace CompanyApi.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class SMSSenderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="smsSender"></param>
        /// <param name="phone"></param>
        /// <param name="code"></param>
        /// <param name="userId"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public static async Task<SMSLog> SendSMSVerifyAccountAsync(this ISMSSender smsSender, string phone, string code, string userId, string culture)
        {
            var message = string.Empty;
            switch (culture)
            {
                case "vi-VN":
                    message = $"Ma xac nhan cua ban la {code}";
                    break;
                case "en-US":
                    message = $"Your verify code is {code}";
                    break;
                default:
                    message = $"Ma xac nhan cua ban la {code}";
                    break;
            }
            var response = await smsSender.SendSMSAsync(phone, message);
            return new SMSLog()
            {
                ObjectId = userId,
                ObjectType = "users",
                Phone = phone,
                Message = message,
                SendTime = DateTime.UtcNow,
                MessageId = response.messageId,
                ErrorCode = response.error_code,
                ErrorDetail = response.error_detail,
                Type = TypeNotification.VerifyAccount.ToString()
            };
        }
    }
}

