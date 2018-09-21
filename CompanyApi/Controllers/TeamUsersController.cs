using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyApi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for TeamUsersController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class TeamUsersController : Controller
    {
        private readonly IRequestClient<IEditTeamUsers, ITeamUsersEdited> _editTeamUsersRequestClient;
        private readonly IRequestClient<IDeleteTeamUsers, ITeamUsersDeleted> _deleteTeamUsersRequestClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="editTeamUsersRequestClient"></param>
        /// <param name="deleteTeamUsersRequestClient"></param>
        public TeamUsersController(
            IRequestClient<IEditTeamUsers, ITeamUsersEdited> editTeamUsersRequestClient,
            IRequestClient<IDeleteTeamUsers, ITeamUsersDeleted> deleteTeamUsersRequestClient)
        {
            _editTeamUsersRequestClient = editTeamUsersRequestClient;
            _deleteTeamUsersRequestClient = deleteTeamUsersRequestClient;
        }

        /// <summary>
        /// edit team users
        /// </summary>
        /// <param name="data">edit info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a  team users</returns>
        /// <response code="200">returns the edited team users</response>
        [HttpPost]
        [ProducesResponseType(typeof(List<TeamUsers>), 200)]
        public async Task<IActionResult> Edit([FromBody]EditTeamUsers data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _editTeamUsersRequestClient.Request(data);
                return Ok(result.TeamUsers);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="CompanyId"></param>
        /// <returns></returns>
        [HttpDelete]
        [ProducesResponseType(typeof(List<TeamUsers>), 200)]
        public async Task<IActionResult> Delete([FromBody]DeleteTeamUsers data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _deleteTeamUsersRequestClient.Request(data);
                return Ok(result.TeamUsers);
            }
            return BadRequest(ModelState);
        }
    }
}