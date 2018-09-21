using Contracts.Commands;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateTeam : IUpdateTeam
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
