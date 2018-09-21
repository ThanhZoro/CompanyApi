using System.Collections.Generic;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class RegisterAccount
    {
        /// <summary>
        /// 
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<string> Position { get; set; } = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LinkLogin { get; set; }
    }
}