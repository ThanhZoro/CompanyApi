using Contracts.Commands;
using Contracts.Models;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateCommonData : ICreateCommonData
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
        [Required(ErrorMessage = "dataTypeRequired")]
        public CommonDataType DataType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "dataKeyRequired")]
        public string DataKey { get; set; }
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
        public string CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
