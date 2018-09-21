using Contracts.Commands;
using Contracts.Models;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IContactLeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ContactLead> Create(ICreateContactLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ContactLead> Update(IUpdateContactLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ContactLead> Delete(IDeleteContactLead data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ContactLead> Upload(IUploadAvatarContactLead data);
    }
}
