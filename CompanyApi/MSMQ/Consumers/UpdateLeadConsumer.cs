using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ApiESReadService.Models;
using Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nest;
using CompanyApi.Data;
using CompanyApi.Repository;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Consumers
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateLeadConsumer : IConsumer<IUpdateLead>
    {
        private readonly ILeadRepository _leadRepository;
        private ElasticClient _esClient;
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadRepository"></param>
        /// <param name="esSettings"></param>
        /// <param name="appcontext"></param>
        public UpdateLeadConsumer(
            ILeadRepository leadRepository,
            IOptions<ElasticSearchSettings> esSettings,
            ApplicationDbContext appcontext)
        {
            _leadRepository = leadRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
            _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IUpdateLead> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var oldLead = _context.Lead.AsQueryable().Where(w => w.Id == context.Message.Id).FirstOrDefault();
            var leadResult = await _leadRepository.Update(context.Message);
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
                            Activity = "updateLead",
                            CreatedBy = lead.UpdatedBy,
                            CreatedAt = lead.UpdatedAt
                        }
                    );
                if (oldLead.Note != lead.Note)
                {
                    await context.Publish<ICreateActivityHistoryLead>(
                        new
                        {
                            lead.CompanyId,
                            LeadId = lead.Id,
                            Type = "note",
                            Activity = lead.Note,
                            CreatedBy = lead.UpdatedBy,
                            CreatedAt = lead.UpdatedAt
                        }
                    );
                }
                if (oldLead.Status != lead.Status)
                {
                    await context.Publish<ICreateActivityHistoryLead>(
                       new
                       {
                           lead.CompanyId,
                           LeadId = lead.Id,
                           Type = "status",
                           Activity = lead.Status,
                           Note = oldLead.Status,
                           CreatedBy = lead.UpdatedBy,
                           CreatedAt = lead.UpdatedAt
                       });
                }
            }
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
