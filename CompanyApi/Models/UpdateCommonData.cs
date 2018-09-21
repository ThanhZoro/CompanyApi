using Contracts.Commands;
using Contracts.Models;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateCommonData : IUpdateCommonData
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "dataValueRequired")]
        public string DataValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Color { get; set; } = "#fff";
        /// <summary>
        /// 
        /// </summary>
        public int Weight { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "dataTypeRequired")]
        public CommonDataType DataType { get; set; }
    }
}
