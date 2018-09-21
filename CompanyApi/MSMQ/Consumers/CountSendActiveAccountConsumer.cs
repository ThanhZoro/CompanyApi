using Contracts.Commands;
using MassTransit;
using MongoDB.Driver;
using CompanyApi.Data;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class CountSendActiveAccountConsumer : IConsumer<ICountSendActiveAccount>
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public CountSendActiveAccountConsumer(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ICountSendActiveAccount> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);

            var data = context.Message;
            long count = 0;
            switch (data.VerifyType)
            {
                case "Email":
                    count = _context.EmailLog.Find(f => f.Type == TypeNotification.VerifyAccount.ToString()
                            && f.ObjectId == data.UserId && f.ObjectType == "users" && f.SendTime >= DateTime.UtcNow.Date).Count();
                    break;
                case "SMS":
                    count = _context.SMSLog.Find(f => f.Type == TypeNotification.VerifyAccount.ToString()
                            && f.ObjectId == data.UserId && f.ObjectType == "users" && f.SendTime >= DateTime.UtcNow.Date).Count();
                    break;
            }
            await context.RespondAsync<ISendActiveAccountCounted>(new
            {
                Count = count
            });
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
