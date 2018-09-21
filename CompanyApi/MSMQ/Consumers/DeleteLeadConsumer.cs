using System;
using System.Diagnostics;
using System.Linq;
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
    public class DeleteLeadConsumer : IConsumer<IDeleteLead>
    {
        private readonly ILeadRepository _leadRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadRepository"></param>
        /// <param name="esSettings"></param>
        public DeleteLeadConsumer(
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
        public async Task Consume(ConsumeContext<IDeleteLead> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var leads = await _leadRepository.Delete(context.Message);
            //index to es
            var response = await _esClient.IndexManyAsync(leads, "leads");
            foreach(var lead in leads)
            {
                await context.Publish<ICreateActivityHistoryLead>(
                    new
                    {
                        lead.CompanyId,
                        LeadId = lead.Id,
                        Type = "edit",
                        Activity = "deleteLead",
                        CreatedBy = lead.UpdatedBy,
                        CreatedAt = lead.UpdatedAt
                    }
                );
            }
            await context.RespondAsync<ILeadDeleted>(new { Ids = leads.Select(s => s.Id) });
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}

