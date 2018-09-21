using Contracts.Commands;
using Contracts.Models;
using CompanyApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<LeadEdited> Create(ICreateLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<LeadEdited> Update(IUpdateLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<Lead>> Delete(IDeleteLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<Lead>> ImportFromExcel(IImportLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        Task<string> Download(string filename, string companyId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Lead> AddSupportStaff(IAddSupportStaff data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Lead> RemoveSupportStaff(IRemoveSupportStaff data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<Lead>> AddStaffInCharge(IAddStaffInCharge data);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<string>> HasPermission(List<string> ids);
    }
}
