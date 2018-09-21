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
    public class CreateProductCategoryConsumer : IConsumer<ICreateProductCategory>
    {
        private IProductCategoryRepository _productCategoryRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productCategoryRepository"></param>
        /// <param name="esSettings"></param>
        public CreateProductCategoryConsumer(
            IProductCategoryRepository productCategoryRepository,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _productCategoryRepository = productCategoryRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<ICreateProductCategory> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var productCategory = await _productCategoryRepository.Create(context.Message);
            await context.RespondAsync(productCategory);
            await _esClient.IndexAsync(productCategory, idx => idx.Index("product_categories"));
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}