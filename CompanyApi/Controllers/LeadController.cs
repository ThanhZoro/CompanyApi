using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2OCRM.Authorization;
using CompanyApi.Models;
using CompanyApi.Repository;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for LeadController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class LeadController : Controller
    {
        private readonly IRequestClient<ICreateLead, LeadEdited> _createLeadRequestClient;
        private readonly IRequestClient<IUpdateLead, LeadEdited> _updateLeadRequestClient;
        private readonly IRequestClient<IDeleteLead, ILeadDeleted> _deleteLeadRequestClient;
        private readonly IRequestClient<IImportLead, ILeadImported> _importLeadClient;
        private readonly IRequestClient<IAddSupportStaff, Lead> _addSupportStaffRequestClient;
        private readonly IRequestClient<IRemoveSupportStaff, Lead> _removeSupportStaffRequestClient;
        private readonly IRequestClient<IAddStaffInCharge, IStaffInChargeAdded> _addStaffInChargeRequestClient;
        private readonly ILeadRepository _leadRepository;
        private readonly ICommonDataRepository _commonDataRepository;

        /// <summary>
        /// contructor LeadController
        /// </summary>
        /// <param name="createLeadRequestClient"></param>
        /// <param name="updateLeadRequestClient"></param>
        /// <param name="deleteLeadRequestClient"></param>
        /// <param name="importLeadClient"></param>
        /// <param name="addSupportStaffRequestClient"></param>
        /// <param name="removeSupportStaffRequestClient"></param>
        /// <param name="addStaffInChargeRequestClient"></param>
        /// <param name="leadRepository"></param>
        /// <param name="commonDataRepository"></param>
        public LeadController(
            IRequestClient<ICreateLead, LeadEdited> createLeadRequestClient,
            IRequestClient<IUpdateLead, LeadEdited> updateLeadRequestClient,
            IRequestClient<IDeleteLead, ILeadDeleted> deleteLeadRequestClient,
            IRequestClient<IImportLead, ILeadImported> importLeadClient,
            IRequestClient<IAddSupportStaff, Lead> addSupportStaffRequestClient,
            IRequestClient<IRemoveSupportStaff, Lead> removeSupportStaffRequestClient,
            IRequestClient<IAddStaffInCharge, IStaffInChargeAdded> addStaffInChargeRequestClient,
            ILeadRepository leadRepository,
            ICommonDataRepository commonDataRepository)
        {
            _createLeadRequestClient = createLeadRequestClient;
            _updateLeadRequestClient = updateLeadRequestClient;
            _deleteLeadRequestClient = deleteLeadRequestClient;
            _importLeadClient = importLeadClient;
            _addSupportStaffRequestClient = addSupportStaffRequestClient;
            _removeSupportStaffRequestClient = removeSupportStaffRequestClient;
            _addStaffInChargeRequestClient = addStaffInChargeRequestClient;
            _leadRepository = leadRepository;
            _commonDataRepository = commonDataRepository;
        }

        /// <summary>
        /// create new lead
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new lead</returns>
        /// <response code="200">returns the newly created lead</response>
        [HttpPut]
        [ProducesResponseType(typeof(Lead), 200)]
        [AccessRight("LEAD_EDIT")]
        public async Task<IActionResult> Create([FromBody]CreateLead data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _createLeadRequestClient.Request(data);
                if (result.IsSuccess == false)
                    return BadRequest(result.DataFail);
                return Ok(result.DataSuccess);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        /// <summary>
        /// update exist lead
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId"></param>
        /// <returns>the updated lead</returns>
        /// <response code="200">returns the updated lead</response>
        [HttpPost]
        [ProducesResponseType(typeof(Lead), 200)]
        [AccessRight("LEAD_EDIT")]
        public async Task<IActionResult> Update([FromBody]UpdateLead data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                var idsHasPermission = await _leadRepository.HasPermission(new List<string>() { data.Id });
                if (idsHasPermission.Count == 0)
                {
                    return Unauthorized();
                }
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _updateLeadRequestClient.Request(data);
                if (result.IsSuccess == false)
                    return BadRequest(result.DataFail);
                return Ok(result.DataSuccess);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete exist lead
        /// </summary>
        /// <param name="data">delete info</param>
        /// <returns>the deleted lead</returns>
        /// <response code="200">returns the deleted lead</response>
        [HttpDelete]
        [ProducesResponseType(typeof(ILeadDeleted), 200)]
        [AccessRight("LEAD_DELETE")]
        public async Task<IActionResult> Delete([FromBody]DeleteLead data)
        {
            if (ModelState.IsValid)
            {
                var idsHasPermission = await _leadRepository.HasPermission(new List<string>(data.Ids));
                if (idsHasPermission.Count == 0)
                {
                    return Unauthorized();
                }
                data.Ids = idsHasPermission;
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                var result = await _deleteLeadRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// import list leads from excel
        /// </summary>
        /// <param name="data">import info</param>
        /// <param name="companyId">company id from header</param>
        /// <returns></returns>
        /// <response code="200">Ok</response>
        [HttpPost]
        [AccessRight("LEAD_IMPORT")]
        public async Task<IActionResult> Import([FromForm]ImportLead data, [FromHeader]string companyId)
        {
            if (ModelState.IsValid)
            {
                data.UserId = User.Claims.FirstOrDefault(s => s.Type == "sub").Value;
                data.CompanyId = companyId;
                data.UserName = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                List<CreateLead> listLeads = new List<CreateLead>();
                using (var memoryStream = new MemoryStream())
                {
                    await data.File.CopyToAsync(memoryStream).ConfigureAwait(false);

                    using (var package = new ExcelPackage(memoryStream))
                    {
                        var worksheet = package.Workbook.Worksheets["Data"];
                        int totalRows = worksheet.Dimension.Rows;
                        int totalCols = worksheet.Dimension.Columns;

                        var commonData = await _commonDataRepository.GetAll(data.CompanyId);
                        var leadStatus = commonData.Where(w => w.DataType == CommonDataType.Status && w.IsDelete == false);
                        var definitionSources = commonData.Where(w => w.DataType == CommonDataType.Source && w.IsDelete == false);
                        var definitionChannels = commonData.Where(w => w.DataType == CommonDataType.Channel && w.IsDelete == false);
                        var genders = commonData.Where(w => w.DataType == CommonDataType.Gender && w.IsDelete == false);
                        var vocatives = commonData.Where(w => w.DataType == CommonDataType.Vocative && w.IsDelete == false);
                        var maritalStatus = commonData.Where(w => w.DataType == CommonDataType.MaritalStatus && w.IsDelete == false);
                        var relationships = commonData.Where(w => w.DataType == CommonDataType.Relationship && w.IsDelete == false);

                        for (int i = 2; i <= totalRows; i++)
                        {
                            var status = worksheet.Cells[i, 6].Value?.ToString();
                            var source = worksheet.Cells[i, 9].Value?.ToString();
                            var channel = worksheet.Cells[i, 10].Value?.ToString();
                            var gender = worksheet.Cells[i, 4].Value?.ToString();
                            var vocative = worksheet.Cells[i, 5].Value?.ToString();
                            var marital = worksheet.Cells[i, 17].Value?.ToString();
                            var relationship = worksheet.Cells[i, 18].Value?.ToString();

                            var haveStatus = string.IsNullOrEmpty(status) ? "" : leadStatus.Where(a => a.DataKey == status.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();
                            var haveSource = string.IsNullOrEmpty(source) ? "" : definitionSources.Where(a => a.DataKey == source.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();
                            var haveChannel = string.IsNullOrEmpty(channel) ? "" : definitionChannels.Where(a => a.DataKey == channel.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();
                            var haveGender = string.IsNullOrEmpty(gender) ? "" : genders.Where(a => a.DataKey == gender.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();
                            var haveVocative = string.IsNullOrEmpty(vocative) ? "" : vocatives.Where(a => a.DataKey == vocative.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();
                            var haveMaritalStatus = string.IsNullOrEmpty(marital) ? "" : maritalStatus.Where(a => a.DataKey == marital.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();
                            var haveRelationship = string.IsNullOrEmpty(relationship) ? "" : relationships.Where(a => a.DataKey == relationship.Split('-').FirstOrDefault()).Select(s => s.Id).FirstOrDefault();

                            if (haveStatus != null && haveSource != null & haveChannel != null && haveGender != null && haveVocative != null && haveMaritalStatus != null && haveRelationship != null)
                            {
                                listLeads.Add(new CreateLead
                                {
                                    FullName = worksheet.Cells[i, 1].Value?.ToString(),
                                    Phone = worksheet.Cells[i, 2].Value?.ToString(),
                                    Email = worksheet.Cells[i, 3].Value?.ToString(),
                                    Gender = haveGender,
                                    Vocative = haveVocative,
                                    Status = haveStatus,
                                    Interest = worksheet.Cells[i, 7].Value?.ToString().Split(',').ToList(),
                                    Note = worksheet.Cells[i, 8].Value?.ToString(),
                                    Source = haveSource,
                                    Channel = haveChannel,
                                    Campaign = worksheet.Cells[i, 11].Value?.ToString(),
                                    Address = worksheet.Cells[i, 12].Value?.ToString(),
                                    IdentityCard = worksheet.Cells[i, 13].Value?.ToString(),
                                    DateOfIssue = (DateTime.TryParse(worksheet.Cells[i, 14].Value?.ToString(), out DateTime dateValue)) ? DateTime.Parse(worksheet.Cells[i, 14].Value.ToString()) : (DateTime?)null,
                                    PlaceOfIssue = worksheet.Cells[i, 15].Value?.ToString(),
                                    CreatedBy = data.UserName,
                                    CompanyId = data.CompanyId,
                                    StaffInCharge = data.StaffInCharge,
                                    SupportStaff = data.SupportStaff,
                                    SocialId = worksheet.Cells[i, 16].Value?.ToString(),
                                    MaritalStatus = haveMaritalStatus,
                                    Relationship = haveRelationship,
                                    Birthday = (DateTime.TryParse(worksheet.Cells[i, 19].Value?.ToString(), out DateTime birthdayValue)) ? DateTime.Parse(worksheet.Cells[i, 19].Value.ToString()) : (DateTime?)null,
                                });
                            }
                        }
                    }
                }
                await _importLeadClient.Request(new { data.StaffInCharge, listLeads });
                return Ok();
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// dowload template import list leads
        /// </summary>
        /// <param name="filename">file name</param>
        /// <param name="companyId">company id from header</param>
        /// <returns>file excel template</returns>
        /// <response code="200">Ok</response>
        [HttpGet]
        [AccessRight("LEAD_IMPORT")]
        public async Task<IActionResult> ImportTemplate([FromQuery]string filename, [FromHeader]string companyId)
        {
            if (filename == null)
                return Content("filename not present");

            var path = await _leadRepository.Download(filename, companyId);

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, GetContentType(path), $"{Path.GetFileName(path)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}");
        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
            {
                {".txt", "text/plain"},
                {".pdf", "application/pdf"},
                {".doc", "application/vnd.ms-word"},
                {".docx", "application/vnd.ms-word"},
                {".xls", "application/vnd.ms-excel"},
                {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
                {".png", "image/png"},
                {".jpg", "image/jpeg"},
                {".jpeg", "image/jpeg"},
                {".gif", "image/gif"},
                {".csv", "text/csv"}
            };
        }

        /// <summary>
        /// add support staff
        /// </summary>
        /// <param name="id">id of lead</param>
        /// <param name="supportStaffId">support staff id</param>
        /// <returns>the updated lead</returns>
        /// <response code="200">returns the updated lead after being added support staff</response>
        [HttpGet]
        [ProducesResponseType(typeof(Lead), 200)]
        [AccessRight("LEAD_EDIT")]
        public async Task<IActionResult> AddSupportStaff([FromQuery]string id, [FromQuery]string supportStaffId)
        {
            var idsHasPermission = await _leadRepository.HasPermission(new List<string>() { id });
            if (idsHasPermission.Count == 0)
            {
                return Unauthorized();
            }
            string updatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
            var result = await _addSupportStaffRequestClient.Request(
                 new
                 {
                     Id = id,
                     SupportStaffId = supportStaffId,
                     UpdatedBy = updatedBy
                 });
            return Ok(result);
        }

        /// <summary>
        /// remove support staff
        /// </summary>
        /// <param name="id">id of lead</param>
        /// <param name="supportStaffId">support staff id</param>
        /// <returns>the updated lead</returns>
        /// <response code="200">returns the updated lead after being removed support staff</response>
        [HttpGet]
        [ProducesResponseType(typeof(Lead), 200)]
        [AccessRight("LEAD_EDIT")]
        public async Task<IActionResult> RemoveSupportStaff([FromQuery]string id, [FromQuery]string supportStaffId)
        {
            var idsHasPermission = await _leadRepository.HasPermission(new List<string>() { id });
            if (idsHasPermission.Count == 0)
            {
                return Unauthorized();
            }
            string updatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
            var result = await _removeSupportStaffRequestClient.Request(
                new
                {
                    Id = id,
                    SupportStaffId = supportStaffId,
                    UpdatedBy = updatedBy
                });
            return Ok(result);
        }

        /// <summary>
        /// add staffInCharge for list leads
        /// </summary>
        /// <param name="data">data info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>list leads updated</returns>
        /// <response code="200">returns list leads updated</response>
        [HttpPost]
        [ProducesResponseType(typeof(List<string>), 200)]
        [AccessRight("LEAD_EDIT")]
        public async Task<IActionResult> AddStaffInCharge([FromBody]AddStaffInCharge data, [FromHeader]string CompanyId)
        {
            var idsHasPermission = await _leadRepository.HasPermission(new List<string>(data.Ids));
            if (idsHasPermission.Count == 0)
            {
                return Unauthorized();
            }
            data.Ids = idsHasPermission;
            data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
            data.CompanyId = CompanyId;
            var result = await _addStaffInChargeRequestClient.Request(data);
            return Ok(result);
        }
    }
}