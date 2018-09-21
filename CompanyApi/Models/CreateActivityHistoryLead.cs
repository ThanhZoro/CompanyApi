using Contracts.Commands;
using System;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateActivityHistoryLead : ICreateActivityHistoryLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string LeadId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Activity { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
    }
}
