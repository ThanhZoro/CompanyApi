using Contracts.Commands;
using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class AddStaffInCharge : IAddStaffInCharge
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Ids { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string StaffInCharge { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CcMail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TeamId { get; set; }
    }
}
