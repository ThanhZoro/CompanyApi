using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Models;
using MongoDB.Driver;
using CompanyApi.Data;
using CompanyApi.Extensions;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class TeamRepository : ITeamRepository
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        public TeamRepository(ApplicationDbContext appcontext)
        {
            _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Team> Create(ICreateTeam data)
        {
            var team = data.Cast<Team>();
            team.CreatedAt = DateTime.UtcNow;
            team.IsDelete = false;
            await _context.Team.InsertOneAsync(team);
            return team;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<Team>> Delete(IDeleteTeam data)
        {
            List<Team> teams = new List<Team>();
            foreach (var id in data.Ids)
            {
                var filter = Builders<Team>.Filter.Eq("Id", id);
                filter &= Builders<Team>.Filter.Eq("CompanyId", data.CompanyId);
                var update = Builders<Team>.Update
                            .Set(s => s.IsDelete, true)
                            .Set(s => s.UpdatedBy, data.UpdatedBy)
                            .CurrentDate(s => s.UpdatedAt);

                var options = new FindOneAndUpdateOptions<Team>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var team = await _context.Team.FindOneAndUpdateAsync(filter, update, options);
                teams.Add(team);
            }
            return teams;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Team> Update(IUpdateTeam data)
        {
            var filter = Builders<Team>.Filter.Eq("Id", data.Id);
            filter &= Builders<Team>.Filter.Eq("CompanyId", data.CompanyId);
            var update = Builders<Team>.Update
                    .Set(s => s.Name, data.Name)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);

            var options = new FindOneAndUpdateOptions<Team>
            {
                ReturnDocument = ReturnDocument.After
            };
            var team = await _context.Team.FindOneAndUpdateAsync(filter, update, options);
            return team;
        }
    }
}
