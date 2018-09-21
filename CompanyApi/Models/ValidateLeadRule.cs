using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidateLeadRule
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> LeadDuplicate { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public List<string> LeadRequire { get; set; } = new List<string>();
    }
}
