using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateLogo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [Required(ErrorMessage = "fileRequired")]
        public IFormFile File { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LogoUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
