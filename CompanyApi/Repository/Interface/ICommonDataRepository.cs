using Contracts.Commands;
using Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICommonDataRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        Task<List<CommonData>> GetAll(string companyId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<CommonData> Create(ICreateCommonData data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<CommonData> Update(IUpdateCommonData data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<CommonData> Delete(IDeleteCommonData data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<CommonData>> CreateDefaultCommonData(ICreateDefaultCommonData data);
    }
}
