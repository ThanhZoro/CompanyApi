using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyApi.Extensions;
using CompanyApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for IContactLeadRepository
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class ContactLeadController : Controller
    {
        private readonly IRequestClient<ICreateContactLead, ContactLead> _createContactLeadRequestClient;
        private readonly IRequestClient<IUpdateContactLead, ContactLead> _updateContactLeadRequestClient;
        private readonly IRequestClient<IDeleteContactLead, ContactLead> _deleteContactLeadRequestClient;
        private readonly IRequestClient<IUploadAvatarContactLead, ContactLead> _uploadAvatarContactLeadRequestClient;

        /// <summary>
        /// contructor ContactLeadController
        /// </summary>
        /// <param name="createContactLeadRequestClient"></param>
        /// <param name="updateContactLeadRequestClient"></param>
        /// <param name="deleteContactLeadRequestClient"></param>
        /// <param name="uploadAvatarContactLeadRequestClient"></param>
        public ContactLeadController(
            IRequestClient<ICreateContactLead, ContactLead> createContactLeadRequestClient,
            IRequestClient<IUpdateContactLead, ContactLead> updateContactLeadRequestClient,
            IRequestClient<IDeleteContactLead, ContactLead> deleteContactLeadRequestClient,
            IRequestClient<IUploadAvatarContactLead, ContactLead> uploadAvatarContactLeadRequestClient)
        {
            _createContactLeadRequestClient = createContactLeadRequestClient;
            _updateContactLeadRequestClient = updateContactLeadRequestClient;
            _deleteContactLeadRequestClient = deleteContactLeadRequestClient;
            _uploadAvatarContactLeadRequestClient = uploadAvatarContactLeadRequestClient;
        }

        /// <summary>
        /// create new contact lead
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new contact lead</returns>
        /// <response code="200">returns the newly created contact lead</response>
        [HttpPut]
        [ProducesResponseType(typeof(ContactLead), 200)]
        public async Task<IActionResult> Create([FromForm]CreateContactLead data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                string avatarUrl = string.Empty;
                if (data.File != null)
                {
                    var uploadResult = CloudinaryUploadExtensions.UploadAvatarLead(data.File);
                    avatarUrl = uploadResult.SecureUri.OriginalString;
                }
                data.AvatarUrl = avatarUrl;
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _createContactLeadRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update contact lead existed
        /// </summary>
        /// <param name="data">update info</param>
        /// <returns>a new contact lead</returns>
        /// <response code="200">returns the newly created contact lead</response>
        [HttpPost]
        [ProducesResponseType(typeof(ContactLead), 200)]
        public async Task<IActionResult> Update([FromBody]UpdateContactLead data)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                var result = await _updateContactLeadRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete contact lead existed
        /// </summary>
        /// <param name="id">id of contact lead</param>
        /// <param name="leadId">id of lead</param>
        /// <returns>the contact lead deleted</returns>
        /// <response code="200">returns the deleted lead</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ContactLead), 200)]
        public async Task<IActionResult> Delete([FromQuery]string id, [FromQuery]string leadId)
        {
            var updatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
            var result = await _deleteContactLeadRequestClient.Request(new
                    {
                        Id = id,
                        LeadId = leadId,
                        UpdatedBy = updatedBy
                    });
            return Ok(result);
        }

        /// <summary>
        /// upload logo for contact lead
        /// </summary>
        /// <param name="data">upload info</param>
        /// <returns>the contact lead uploaded</returns>
        /// <response code="200">returns the contact lead uploaded</response>
        [HttpPost]
        [ProducesResponseType(typeof(ContactLead), 200)]
        public async Task<IActionResult> Upload([FromForm]UploadAvatarContactLead data)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                string avatarUrl = string.Empty;
                if (data.File != null)
                {
                    var uploadResult = CloudinaryUploadExtensions.UploadAvatarLead(data.File);
                    avatarUrl = uploadResult.SecureUri.OriginalString;
                }
                var result = await _uploadAvatarContactLeadRequestClient.Request(
                   new
                   {
                       data.Id,
                       AvatarUrl = avatarUrl,
                       data.UpdatedBy
                   });
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
    }
}