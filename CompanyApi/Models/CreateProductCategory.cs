using Contracts.Commands;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateProductCategory : ICreateProductCategory
    {
        /// <summary>
        /// 
        /// </summary>
        public string CompanyId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string CategoryCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ParentId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CreatedBy { get; set; }
    }
}
