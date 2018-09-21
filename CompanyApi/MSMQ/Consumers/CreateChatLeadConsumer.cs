using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ApiESReadService.Models;
using Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Options;
using Nest;
using CompanyApi.Repository;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateChatLeadConsumer : IConsumer<ICreateChatLead>
    {
        private readonly IChatLeadRepository _chatLeadRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatLeadRepository"></param>
        /// <param name="esSettings"></param>
        public CreateChatLeadConsumer(
            IChatLeadRepository chatLeadRepository,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _chatLeadRepository = chatLeadRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ICreateChatLead> context)
        {
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .WriteTo.Console(theme: ConsoleTheme.None)
                 .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var chatLead = await _chatLeadRepository.Create(context.Message);
            //index to es
            var response = await _esClient.IndexAsync(chatLead, idx => idx.Index("chat_leads"));
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}

