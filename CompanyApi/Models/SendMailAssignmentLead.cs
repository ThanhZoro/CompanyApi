using Contracts.Models;
using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class SendMailAssignmentLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public List<Lead> Leads { get; set; } = new List<Lead>();

        /// <summary>
        /// 
        /// </summary>
        public string StaffInCharge { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CcMail { get; set; }
    }
}
