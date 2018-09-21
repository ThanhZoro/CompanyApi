using Contracts.Commands;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateChatLead : ICreateChatLead
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string LeadId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
    }
}
