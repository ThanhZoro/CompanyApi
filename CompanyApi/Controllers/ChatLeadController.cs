using System.Linq;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyApi.Models;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for ChatLeadController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class ChatLeadController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IPublishEndpoint _publishEndpoint;

        /// <summary>
        /// contructor ChatLeadController
        /// </summary>
        /// <param name="publishEndpoint"></param>
        public ChatLeadController(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        /// <summary>
        /// create new chat lead
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new chat lead</returns>
        /// <response code="200">returns the newly created chat lead</response>
        [HttpPut]
        [ProducesResponseType(typeof(ChatLead), 200)]
        public async Task<IActionResult> Create([FromBody]CreateChatLead data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.UserId = User.Claims.FirstOrDefault(s => s.Type == "sub").Value;
                data.CompanyId = CompanyId;
                await _publishEndpoint.Publish<ICreateChatLead>(data);
                return Ok();
            }
            return BadRequest(ModelState);
        }
    }
}