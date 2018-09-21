using Contracts.Commands;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateProduct : IUpdateProduct
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
        public string ProductCode { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProductCategoryId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "priceInvalid")]
        public double Price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Columns { get; set; } = new List<string>();
    }
}
