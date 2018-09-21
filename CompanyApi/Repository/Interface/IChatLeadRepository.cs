using Contracts.Commands;
using Contracts.Models;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IChatLeadRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ChatLead> Create(ICreateChatLead data);
    }
}
