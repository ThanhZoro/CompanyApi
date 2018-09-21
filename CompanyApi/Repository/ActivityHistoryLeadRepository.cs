using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Models;
using CompanyApi.Data;
using CompanyApi.Extensions;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class ActivityHistoryLeadRepository : IActivityHistoryLeadRepository
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        public ActivityHistoryLeadRepository(ApplicationDbContext appcontext)
        {
            _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ActivityHistoryLead> Create(ICreateActivityHistoryLead data)
        {
            var activityHistory = data.Cast<ActivityHistoryLead>();
            activityHistory.CreatedAt = DateTime.UtcNow;
            await _context.ActivityHistoryLead.InsertOneAsync(activityHistory);
            return activityHistory;
        }
    }
}
