using Contracts.Commands;
using Contracts.Models;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateMailSettings : IUpdateMailSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public MailSettings MailSettings { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
