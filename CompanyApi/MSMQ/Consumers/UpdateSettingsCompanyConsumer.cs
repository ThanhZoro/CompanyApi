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
    public class UpdateSettingsCompanyConsumer : IConsumer<IUpdateSettingsCompany>
    {
        private readonly ICompanyRepository _companyRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyRepository"></param>
        /// <param name="esSettings"></param>
        public UpdateSettingsCompanyConsumer(
            ICompanyRepository companyRepository,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _companyRepository = companyRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IUpdateSettingsCompany> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var company = await _companyRepository.UpdateSettings(context.Message);
            await context.RespondAsync(company);
            //index to es
            var response = await _esClient.IndexAsync(company, idx => idx.Index("companies"));
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}