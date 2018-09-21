using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using CompanyApi.Data;
using CompanyApi.Extensions;
using CompanyApi.Services;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class SendSMSConsumer : IConsumer<ISendSMS>
    {
        private ApplicationDbContext _context;
        private readonly ISMSSender _smsSender;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="smsSender"></param>
        public SendSMSConsumer(ApplicationDbContext context, ISMSSender smsSender)
        {
            _context = context;
            _smsSender = smsSender;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ISendSMS> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);

            var data = context.Message;
            var smsLog = new SMSLog();
            switch (data.TypeNotification)
            {
                case TypeNotification.VerifyAccount:
                    smsLog = await _smsSender.SendSMSVerifyAccountAsync(data.Data.Phone, data.Data.Message.ToString(), data.ObjectId, data.Culture);
                    break;
            }
            if (smsLog != null)
                await _context.SMSLog.InsertOneAsync(smsLog);

            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}