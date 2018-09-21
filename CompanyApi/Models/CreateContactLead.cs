using Contracts.Commands;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateContactLead : ICreateContactLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Relationship { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AvatarUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LeadId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IFormFile File { get; set; }
    }
}
