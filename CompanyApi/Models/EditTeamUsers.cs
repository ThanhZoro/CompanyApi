using Contracts.Commands;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class EditTeamUsers : IEditTeamUsers
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public List<string> TeamIds { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public List<string> UserIds { get; set; } = new List<string>();
    }
}
