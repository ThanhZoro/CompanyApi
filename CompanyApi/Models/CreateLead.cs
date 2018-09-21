using Contracts.Commands;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateLead : ICreateLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Vocative { get; set; }
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
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Interest { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
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
        public string Gender { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? Birthday { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IdentityCard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? DateOfIssue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PlaceOfIssue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MaritalStatus { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Relationship { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string SocialId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Channel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Campaign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IFormFile File { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AvatarUrl { get; set; }

        /// <summary>
        /// gtm info
        /// </summary>
        public string TrackingId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Tid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Referrer { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string GoalId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LeadSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Int64 GoalValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UtmCampaign { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UtmMedium { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UtmSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UtmTerm { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UtmContent { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TeamId { get; set; }
    }
}
