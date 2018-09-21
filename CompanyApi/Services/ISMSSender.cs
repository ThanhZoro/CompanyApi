using System.Threading.Tasks;
using static CompanyApi.Services.SMSSender;

namespace CompanyApi.Services
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISMSSender
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task<ApiBulkReturn> SendSMSAsync(string phone, string message);
    }
}
