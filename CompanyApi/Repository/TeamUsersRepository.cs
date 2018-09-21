using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using CompanyApi.Data;
using CompanyApi.Extensions;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class TeamUsersRepository : ITeamUsersRepository
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        public TeamUsersRepository(ApplicationDbContext appcontext)
        {
            _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<TeamUsers>> Edit(IEditTeamUsers data)
        {
            var teamUsers = new List<TeamUsers>();
            data.TeamIds = data.TeamIds.Distinct().ToList();
            foreach (var userId in data.UserIds)
            {
                var existData = await _context.TeamUsers.Find(f => f.CompanyId == data.CompanyId && f.UserId == userId).FirstOrDefaultAsync();
                if (existData != null)
                {
                    var newTeamIds = data.TeamIds.Union(existData.TeamIds).ToList();
                    var filter = Builders<TeamUsers>.Filter.Eq("Id", existData.Id);
                    var update = Builders<TeamUsers>.Update
                            .Set(s => s.TeamIds, newTeamIds)
                            .Set(s => s.UpdatedBy, data.CreatedBy)
                            .CurrentDate(s => s.UpdatedAt);

                    var options = new FindOneAndUpdateOptions<TeamUsers>
                    {
                        ReturnDocument = ReturnDocument.After
                    };
                    var teamUser = await _context.TeamUsers.FindOneAndUpdateAsync(filter, update, options);
                    teamUsers.Add(teamUser);
                }
                else
                {
                    var teamUser = new TeamUsers()
                    {
                        CompanyId = data.CompanyId,
                        TeamIds = data.TeamIds,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = data.CreatedBy
                    };
                    await _context.TeamUsers.InsertOneAsync(teamUser);
                    teamUsers.Add(teamUser);
                }
            }
            return teamUsers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<TeamUsers>> Delete(IDeleteTeamUsers data)
        {
            var teamUsers = new List<TeamUsers>();
            foreach(var userId in data.UserIds)
            {
                var existData = await _context.TeamUsers.Find(f => f.CompanyId == data.CompanyId && f.UserId == userId).FirstOrDefaultAsync();
                if(existData != null)
                {
                    var teamIds = existData.TeamIds ?? new List<string>();
                    var newTeamIds = teamIds.Where(w => !data.TeamIds.Contains(w)).ToList();
                    var filter = Builders<TeamUsers>.Filter.Eq("Id", existData.Id);
                    var update = Builders<TeamUsers>.Update
                            .Set(s => s.TeamIds, newTeamIds)
                            .Set(s => s.UpdatedBy, data.UpdatedBy)
                            .CurrentDate(s => s.UpdatedAt);

                    var options = new FindOneAndUpdateOptions<TeamUsers>
                    {
                        ReturnDocument = ReturnDocument.After
                    };
                    var teamUser = await _context.TeamUsers.FindOneAndUpdateAsync(filter, update, options);
                    teamUsers.Add(teamUser);
                }
            }
            return teamUsers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamIds"></param>
        /// <param name="companyId"></param>
        /// <param name="updatedBy"></param>
        /// <returns></returns>
        public async Task<List<TeamUsers>> Delete(List<string> teamIds, string companyId, string updatedBy)
        {
            var teamUsers = new List<TeamUsers>();
            var filterSearch = Builders<TeamUsers>.Filter.Eq(e => e.CompanyId, companyId);
            filterSearch &= Builders<TeamUsers>.Filter.AnyIn(e => e.TeamIds, teamIds);
            var result = await _context.TeamUsers.Find(filterSearch).ToListAsync();
            foreach (var item in result)
            {
                var filter = Builders<TeamUsers>.Filter.Eq("Id", item.Id);
                var update = Builders<TeamUsers>.Update
                        .PullAll(s => s.TeamIds, teamIds)
                        .Set(s => s.UpdatedBy, updatedBy)
                        .CurrentDate(s => s.UpdatedAt);

                var options = new FindOneAndUpdateOptions<TeamUsers>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var teamUser = await _context.TeamUsers.FindOneAndUpdateAsync(filter, update, options);
                teamUsers.Add(teamUser);
            }
            return teamUsers;
        }
    }
}
