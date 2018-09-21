using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using CompanyApi.Data;
using CompanyApi.Extensions;
using CompanyApi.Models;
using CompanyApi.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class SendMailConsumer : IConsumer<ISendMail>
    {
        private ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly IRequestClient<IGetUsers, IUsersGetted> _getUsersRequestClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="emailSender"></param>
        /// <param name="getUsersRequestClient"></param>
        public SendMailConsumer(
            ApplicationDbContext context, 
            IEmailSender emailSender, 
            IRequestClient<IGetUsers, IUsersGetted> getUsersRequestClient)
        {
            _context = context;
            _emailSender = emailSender;
            _getUsersRequestClient = getUsersRequestClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ISendMail> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);

            var data = context.Message;
            var emailLog = new EmailLog();
            switch (data.TypeNotification)
            {
                case TypeNotification.ForgotPassword:
                    emailLog = await _emailSender.SendEmailForgotPasswordAsync(data.Data.Receiver, data.Data.Body.ToString(), data.ObjectId, data.Culture);
                    break;
                case TypeNotification.VerifyAccount:
                    emailLog = await _emailSender.SendEmailConfirmationAsync(data.Data.Receiver, data.Data.Body.ToString(), data.ObjectId, data.Culture);
                    break;
                case TypeNotification.RegisterAccount:
                    var dataRegister = (data.Data.Body as JObject).ToObject<RegisterAccount>();
                    emailLog = await _emailSender.SendEmailRegistrationAsync(data.Data.Receiver, dataRegister, data.ObjectId, data.Culture);
                    break;
            }
            if (emailLog != null)
                await _context.EmailLog.InsertOneAsync(emailLog);
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
