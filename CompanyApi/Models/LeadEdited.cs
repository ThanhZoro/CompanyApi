using Contracts.Commands;
using Contracts.Models;
using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class LeadEdited
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public EditLeadFail DataFail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Lead DataSuccess { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    public class EditLeadFail
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
