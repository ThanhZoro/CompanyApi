using Contracts.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateLead : IUpdateLead
    {
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Campaign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SocialId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Relationship { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MaritalStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PlaceOfIssue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateOfIssue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TeamId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AgencyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Interest { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Vocative { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> SupportStaff { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public List<string> Columns { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
    }
}
