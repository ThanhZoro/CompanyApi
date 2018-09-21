using Contracts.Commands;
using Contracts.Models;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IActivityHistoryLeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ActivityHistoryLead> Create(ICreateActivityHistoryLead data);
    }
}
