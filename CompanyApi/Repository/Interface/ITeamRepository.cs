using Contracts.Commands;
using Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITeamRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Team> Create(ICreateTeam data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Team> Update(IUpdateTeam data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<Team>> Delete(IDeleteTeam data);
    }
}
