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
    public class DeleteProductCategoryConsumer : IConsumer<IDeleteProductCategory>
    {
        private IProductCategoryRepository _productCategoryRepository;
        private ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productCategoryRepository"></param>
        /// <param name="esSettings"></param>
        public DeleteProductCategoryConsumer(
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
        public async Task Consume(ConsumeContext<IDeleteProductCategory> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var productCategories = await _productCategoryRepository.Delete(context.Message);
            await context.RespondAsync<IProductCategoryDeleted>(new { Ids = productCategories.Select(s => s.Id) });
            //index to es
            if(productCategories.Count > 0)
                await _esClient.IndexManyAsync(productCategories, "product_categories");
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}

