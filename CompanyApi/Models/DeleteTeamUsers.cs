using Contracts.Commands;
using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteTeamUsers : IDeleteTeamUsers
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> UserIds { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public List<string> TeamIds { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
