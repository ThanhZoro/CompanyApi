using Contracts.Commands;

namespace CompanyApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateContactLead : IUpdateContactLead
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Phone { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Relationship { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string UpdatedBy { get; set; }
    }
}
