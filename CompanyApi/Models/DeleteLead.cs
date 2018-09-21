using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteLead
    {
        /// <summary>
        /// 
        /// </summary>
        public List<string> Ids { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
