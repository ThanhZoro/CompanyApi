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
    public class ImportLeadConsumer : IConsumer<IImportLead>
    {
        private readonly ILeadRepository _leadRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadRepository"></param>
        /// <param name="esSettings"></param>
        public ImportLeadConsumer(
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
        public async Task Consume(ConsumeContext<IImportLead> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);

            await context.RespondAsync<ILeadImported>(new { context.Message.UserId });
            var listLead = await _leadRepository.ImportFromExcel(context.Message);
            if (listLead.Count > 0)
            {
                //index to es
                var response = await _esClient.IndexManyAsync(listLead, "leads");
            }
            foreach (var lead in listLead)
            {
                await context.Publish<ICreateActivityHistoryLead>(
                    new
                    {
                        lead.CompanyId,
                        LeadId = lead.Id,
                        Type = "edit",
                        Activity = "createLead",
                        lead.CreatedBy,
                        lead.CreatedAt
                    }
                );
                if (!string.IsNullOrEmpty(lead.Note))
                {
                    await context.Publish<ICreateActivityHistoryLead>(
                        new
                        {
                            lead.CompanyId,
                            LeadId = lead.Id,
                            Type = "note",
                            Activity = lead.Note,
                            lead.CreatedBy,
                            CreatedAt = lead.CreatedAt.AddMinutes(1)
                        });
                }
                if (!string.IsNullOrEmpty(lead.Status))
                {
                    await context.Publish<ICreateActivityHistoryLead>(
                        new
                        {
                            lead.CompanyId,
                            LeadId = lead.Id,
                            Type = "status",
                            Activity = lead.Status,
                            lead.CreatedBy,
                            CreatedAt = lead.CreatedAt.AddMinutes(1)
                        });
                }
            }
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
