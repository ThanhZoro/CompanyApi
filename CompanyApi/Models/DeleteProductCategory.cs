using Contracts.Commands;
using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class DeleteProductCategory : IDeleteProductCategory
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
        public string UpdatedBy { get; set; }
    }
}
