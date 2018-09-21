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
    public class DeleteTeamConsumer : IConsumer<IDeleteTeam>
    {
        private readonly ITeamRepository _teamRepository;
        private ElasticClient _esClient;
        private readonly ITeamUsersRepository _teamUserRepository;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamRepository"></param>
        /// <param name="esSettings"></param>
        /// <param name="teamUserRepository"></param>
        public DeleteTeamConsumer(
            ITeamRepository teamRepository,
            IOptions<ElasticSearchSettings> esSettings,
            ITeamUsersRepository teamUserRepository)
        {
            _teamRepository = teamRepository;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            _esClient = new ElasticClient(connSettings);
            _teamUserRepository = teamUserRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Consume(ConsumeContext<IDeleteTeam> context)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .CreateLogger();
            var start = Stopwatch.GetTimestamp();
            Log.Information("Received command {CommandName}-{MessageId}: {@Messages}", GetType().Name, context.MessageId, context.Message);
            var teams = await _teamRepository.Delete(context.Message);
            var ids = teams.Select(s => s.Id).ToList();
            await context.RespondAsync<ITeamDeleted>(new { Ids = ids });
            //index to es
            if (teams.Count > 0)
                await _esClient.IndexManyAsync(teams, "teams");
            var teamUsers = await _teamUserRepository.Delete(ids, context.Message.CompanyId, context.Message.UpdatedBy);
            if(teamUsers.Count > 0)
                await _esClient.IndexManyAsync(teamUsers, "team_users");
            Log.Information("Completed command {CommandName}-{MessageId} {ExecuteTime}ms", GetType().Name, context.MessageId, (Stopwatch.GetTimestamp() - start) * 1000 / (double)Stopwatch.Frequency);
        }
    }
}