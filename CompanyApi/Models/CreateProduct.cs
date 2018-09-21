using Contracts.Commands;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateProduct : ICreateProduct
    {
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
        public string CreatedBy { get; set; }
    }
}
