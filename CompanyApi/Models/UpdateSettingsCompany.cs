using Contracts.Commands;
using Contracts.Models;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateSettingsCompany : IUpdateSettingsCompany
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public OtherSettings OtherSettings { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
