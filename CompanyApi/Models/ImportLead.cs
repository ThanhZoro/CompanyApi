using Contracts.Commands;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ImportLead : IImportLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string StaffInCharge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> SupportStaff { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public IFormFile File { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<ICreateLead> ListLeads { get; set; } = new List<ICreateLead>();
        /// <summary>
        /// 
        /// </summary>

        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
    }
}