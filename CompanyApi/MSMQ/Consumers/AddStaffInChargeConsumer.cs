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
    public class AddStaffInChargeConsumer : IConsumer<IAddStaffInCharge>
    {

        private readonly ILeadRepository _leadRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leadRepository"></param>
        /// <param name="esSettings"></param>
        public AddStaffInChargeConsumer(
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
        public async Task Consume(ConsumeContext<IAddStaffInCharge> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var leads = await _leadRepository.AddStaffInCharge(context.Message);
            foreach(var lead in leads)
            {
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
                await context.Publish<ICreateActivityHistoryLead>(
                   new
                   {
                       lead.CompanyId,
                       LeadId = lead.Id,
                       Type = "assignment",
                       Activity = lead.StaffInCharge,
                       lead.CreatedBy,
                       lead.CreatedAt
                   });
            }
            await context.RespondAsync<IStaffInChargeAdded>(new { Ids = leads.Select(s => s.Id) });
            if (leads.Count > 0)
            {
                //index to es
                var response = await _esClient.IndexManyAsync(leads, "leads");
                await context.Publish<ISendMail>(
                    new
                    {
                        TypeNotification = TypeNotification.AssignmentLead,
                        Data = new DataSendMail()
                        {
                            Body = new
                            {
                                context.Message.CompanyId,
                                Leads = leads,
                                context.Message.StaffInCharge,
                                context.Message.CcMail
                            }
                        }
                    });
            }
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}

