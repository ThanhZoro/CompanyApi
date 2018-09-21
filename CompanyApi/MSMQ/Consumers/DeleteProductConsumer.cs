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
    public class DeleteProductConsumer : IConsumer<IDeleteProduct>
    {
        private IProductRepository _productRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productRepository"></param>
        /// <param name="esSettings"></param>
        public DeleteProductConsumer(
            IProductRepository productRepository,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _productRepository = productRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IDeleteProduct> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var data = context.Message;
            var products = await _productRepository.Delete(data);
            await context.RespondAsync<IProductDeleted>(new { Ids = products.Select(s => s.Id) });
            //index to es
            if (products.Count > 0)
                await _esClient.IndexManyAsync(products, "products");
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}