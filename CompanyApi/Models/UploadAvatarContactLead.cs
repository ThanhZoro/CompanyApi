using Contracts.Commands;
using Microsoft.AspNetCore.Http;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UploadAvatarContactLead : IUploadAvatarContactLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AvatarUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IFormFile File { get; set; }
    }
}
