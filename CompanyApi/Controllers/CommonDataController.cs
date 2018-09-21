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
    /// summary for CommonDataController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class CommonDataController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IRequestClient<ICreateCommonData, CommonData> _createCommonDataRequestClient;
        private readonly IRequestClient<IUpdateCommonData, CommonData> _updateCommonDataRequestClient;
        private readonly IRequestClient<IDeleteCommonData, CommonData> _deleteCommonDataRequestClient;

        /// <summary>
        /// contructor CommonDataController
        /// </summary>
        /// <param name="createCommonDataRequestClient"></param>
        /// <param name="updateCommonDataRequestClient"></param>
        /// <param name="deleteCommonDataRequestClient"></param>
        public CommonDataController(
            IRequestClient<ICreateCommonData, CommonData> createCommonDataRequestClient,
            IRequestClient<IUpdateCommonData, CommonData> updateCommonDataRequestClient,
            IRequestClient<IDeleteCommonData, CommonData> deleteCommonDataRequestClient)
        {
            _createCommonDataRequestClient = createCommonDataRequestClient;
            _updateCommonDataRequestClient = updateCommonDataRequestClient;
            _deleteCommonDataRequestClient = deleteCommonDataRequestClient;
        }

        /// <summary>
        /// create new common data
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new common data</returns>
        /// <response code="200">returns the newly created common data</response>
        [HttpPut]
        [ProducesResponseType(typeof(CommonData), 200)]
        public async Task<IActionResult> Create([FromBody]CreateCommonData data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.UpdatedBy = data.CreatedBy;
                data.CompanyId = CompanyId;
                var result = await _createCommonDataRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update existed common data
        /// </summary>
        /// <param name="data">update info</param>
        /// <returns>the updated common data</returns>
        /// <response code="200">returns the updated common data</response>
        [HttpPost]
        [ProducesResponseType(typeof(CommonData), 200)]
        public async Task<IActionResult> Update([FromBody]UpdateCommonData data)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                var result = await _updateCommonDataRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete existed common data
        /// </summary>
        /// <param name="Id">id of common data</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the deleted common data</returns>
        /// <response code="200">returns the deleted common data</response>
        [HttpDelete]
        [ProducesResponseType(typeof(CommonData), 200)]
        public async Task<IActionResult> Delete([FromQuery]string Id, [FromHeader]string CompanyId)
        {
            var updatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
            var result = await _deleteCommonDataRequestClient.Request(new { Id, UpdatedBy = updatedBy });
            return Ok(result);
        }
    }
}