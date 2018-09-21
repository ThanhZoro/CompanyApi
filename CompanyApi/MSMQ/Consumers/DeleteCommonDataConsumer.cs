using ApiESReadService.Models;
using Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Options;
using Nest;
using CompanyApi.Repository;
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
    public class DeleteCommonDataConsumer : IConsumer<IDeleteCommonData>
    {
        private readonly ICommonDataRepository _commonDataRepository;
        private readonly ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonDataRepository"></param>
        /// <param name="esSettings"></param>
        public DeleteCommonDataConsumer(
            ICommonDataRepository commonDataRepository,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _commonDataRepository = commonDataRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IDeleteCommonData> context)
        {
            Log.Logger = new LoggerConfiguration()
                 .MinimumLevel.Debug()
                 .WriteTo.Console(theme: ConsoleTheme.None)
                 .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var commonData = await _commonDataRepository.Delete(context.Message);
            await context.RespondAsync(commonData);
            //index to es
            var response = await _esClient.IndexAsync(commonData, idx => idx.Index("common_data"));
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}
