using Contracts.Commands;
using Contracts.Models;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICompanyRepository
    {
        /// <summary>
        /// update logo image
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Company> UpdateLogo(IUploadLogoCompany data);
        /// <summary>
        /// update company general info
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Company> UpdateInfo(IUpdateCompany data);
        /// <summary>
        /// update other settings company
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Company> UpdateSettings(IUpdateSettingsCompany data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Company> UpdateMailSettings(IUpdateMailSettings data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Company> Create(ICreateCompany data);
    }
}
