using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ApiESReadService.Models;
using Contracts.Commands;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
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
    public class EditTeamUsersConsumer : IConsumer<IEditTeamUsers>
    {
        private ITeamUsersRepository _teamUsersRepository;
        private ElasticClient _esClient;
        private readonly IDistributedCache _distributedCache;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamUsersRepository"></param>
        /// <param name="esSettings"></param>
        /// <param name="distributedCache"></param>
        public EditTeamUsersConsumer(
            ITeamUsersRepository teamUsersRepository,
            IOptions<ElasticSearchSettings> esSettings,
            IDistributedCache distributedCache)
        {
            _teamUsersRepository = teamUsersRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
            _distributedCache = distributedCache;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IEditTeamUsers> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var teamUsers = await _teamUsersRepository.Edit(context.Message);
            await context.RespondAsync<ITeamUsersEdited>(new { TeamUsers = teamUsers });
            if(teamUsers.Count > 0)
                await _esClient.IndexManyAsync(teamUsers, "team_users");
            foreach (var teamUser in teamUsers)
            {
                await _distributedCache.RemoveAsync($"access-right-{teamUser.CompanyId}-{teamUser.UserId}");
            }
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}