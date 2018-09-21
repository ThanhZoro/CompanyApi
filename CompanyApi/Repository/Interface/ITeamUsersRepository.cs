using Contracts.Commands;
using Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITeamUsersRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<TeamUsers>> Edit(IEditTeamUsers data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<TeamUsers>> Delete(IDeleteTeamUsers data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamIds"></param>
        /// <param name="companyId"></param>
        /// <param name="updatedBy"></param>
        /// <returns></returns>
        Task<List<TeamUsers>> Delete(List<string> teamIds, string companyId, string updatedBy);
    }
}
