using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CompanyApi.Extensions;
using CompanyApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for CompanyController
    /// </summary>
    [Authorize]
    [Route("api/company/[action]")]
    public class CompanyController : Controller
    {
        private readonly IRequestClient<IUploadLogoCompany, Company> _uploadLogoCompanyRequestClient;
        private readonly IRequestClient<IUpdateCompany, Company> _updateCompanyRequestClient;
        private readonly IRequestClient<IUpdateSettingsCompany, Company> _updateSettingsCompanyRequestClient;
        private readonly IRequestClient<IUpdateMailSettings, Company> _updateMailSettingsCompanyRequestClient;

        /// <summary>
        /// contructor CompanyController
        /// </summary>
        /// <param name="uploadLogoCompanyRequestClient"></param>
        /// <param name="updateCompanyRequestClient"></param>
        /// <param name="updateSettingsCompanyRequestClient"></param>
        /// <param name="updateMailSettingsCompanyRequestClient"></param>
        public CompanyController(
            IRequestClient<IUploadLogoCompany, Company> uploadLogoCompanyRequestClient,
            IRequestClient<IUpdateCompany, Company> updateCompanyRequestClient,
            IRequestClient<IUpdateSettingsCompany, Company> updateSettingsCompanyRequestClient,
            IRequestClient<IUpdateMailSettings, Company> updateMailSettingsCompanyRequestClient)
        {
            _uploadLogoCompanyRequestClient = uploadLogoCompanyRequestClient;
            _updateCompanyRequestClient = updateCompanyRequestClient;
            _updateSettingsCompanyRequestClient = updateSettingsCompanyRequestClient;
            _updateMailSettingsCompanyRequestClient = updateMailSettingsCompanyRequestClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult EncryptString([FromQuery]string text)
        {
            var key = Encoding.UTF8.GetBytes("E546C8DF278CD5931069B522E695D4F2");
            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Ok(Convert.ToBase64String(result));
                    }
                }
            }
        }

        /// <summary>
        /// update company info
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company Id from header</param>
        /// <returns>the updated company</returns>
        /// <response code="200">returns the updated company</response>
        [HttpPost]
        [ProducesResponseType(typeof(Company), 200)]
        public async Task<IActionResult> UpdateInfo([FromBody]Company data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                var result = await _updateCompanyRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update logo of company
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the updated company</returns>
        /// <response code="200">returns the updated company</response>
        [HttpPost]
        [ProducesResponseType(typeof(Company), 200)]
        public async Task<IActionResult> UpdateLogo([FromForm]UpdateLogo data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                var uploadResult = CloudinaryUploadExtensions.UploadImageCompany(data.File);
                data.Id = CompanyId;
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.LogoUrl = uploadResult.SecureUri.OriginalString;
                var result = await _uploadLogoCompanyRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update other settings company
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the updated company</returns>
        /// <response code="200">returns the updated company</response>
        [HttpPost]
        [ProducesResponseType(typeof(Company), 200)]
        public async Task<IActionResult> UpdateSettings([FromBody]UpdateSettingsCompany data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.Id = CompanyId;
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                var result = await _updateSettingsCompanyRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update mail settings company
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the updated company</returns>
        /// <response code="200">returns the updated company</response>
        [HttpPost]
        [ProducesResponseType(typeof(Company), 200)]
        public async Task<IActionResult> UpdateMailSettings([FromBody]UpdateMailSettings data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.Id = CompanyId;
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                var result = await _updateMailSettingsCompanyRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
    }
}