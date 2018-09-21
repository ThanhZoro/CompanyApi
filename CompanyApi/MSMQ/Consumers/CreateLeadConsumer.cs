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
    public class CreateLeadConsumer : IConsumer<ICreateLead>
    {
        private readonly ILeadRepository _leadRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadRepository"></param>
        /// <param name="esSettings"></param>
        public CreateLeadConsumer(
            ILeadRepository leadRepository,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _leadRepository = leadRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ICreateLead> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var leadResult = await _leadRepository.Create(context.Message);
            await context.RespondAsync(leadResult);
            if (leadResult.IsSuccess)
            {
                var lead = leadResult.DataSuccess;
                //index to es
                var response = await _esClient.IndexAsync(lead, idx => idx.Index("leads"));
                
                await context.Publish<ICreateActivityHistoryLead>(
                    new
                    {
                        lead.CompanyId,
                        LeadId = lead.Id,
                        Type = "edit",
                        Activity = "createLead",
                        lead.CreatedBy,
                        lead.CreatedAt
                    });
                
            }
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
