using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyApi.Models;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for TeamController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class TeamController : Controller
    {
        private readonly IRequestClient<ICreateTeam, Team> _createTeamRequestClient;
        private readonly IRequestClient<IUpdateTeam, Team> _updateTeamRequestClient;
        private readonly IRequestClient<IDeleteTeam, ITeamDeleted> _deleteTeamRequestClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="createTeamRequestClient"></param>
        /// <param name="updateTeamRequestClient"></param>
        /// <param name="deleteTeamRequestClient"></param>
        public TeamController(
            IRequestClient<ICreateTeam, Team> createTeamRequestClient,
            IRequestClient<IUpdateTeam, Team> updateTeamRequestClient,
            IRequestClient<IDeleteTeam, ITeamDeleted> deleteTeamRequestClient)
        {
            _createTeamRequestClient = createTeamRequestClient;
            _updateTeamRequestClient = updateTeamRequestClient;
            _deleteTeamRequestClient = deleteTeamRequestClient;
        }

        /// <summary>
        /// create team
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new  team</returns>
        /// <response code="200">returns the newly created team</response>
        [HttpPut]
        [ProducesResponseType(typeof(Team), 200)]
        public async Task<IActionResult> Create([FromBody]CreateTeam data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _createTeamRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update team
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the updated team</returns>
        /// <response code="200">returns the updated team</response>
        [HttpPost]
        [ProducesResponseType(typeof(Team), 200)]
        public async Task<IActionResult> Update([FromBody]UpdateTeam data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _updateTeamRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete team
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the deleted team</returns>
        /// <response code="200">returns the deleted team</response>
        [HttpDelete]
        [ProducesResponseType(typeof(Team), 200)]
        public async Task<IActionResult> Delete([FromBody]DeleteTeam data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _deleteTeamRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
    }
}