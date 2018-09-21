using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Models;
using MongoDB.Driver;
using CompanyApi.Data;
using CompanyApi.Extensions;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        public CompanyRepository(ApplicationDbContext appcontext)
        {
            _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Company> Create(ICreateCompany data)
        {
            var company = data.Cast<Company>();
            company.CreatedAt = DateTime.UtcNow;
            company.IsDelete = false;
            company.IsActive = true;
            company.OtherSettings = new OtherSettings()
            {
                LeadRequire = new List<string>() { "fullName", "phone" },
                LanguageDefault = data.LanguageDefault,
                TimeZone = data.LanguageDefault == "vi-VN" ? "SE Asia Standard Time" : "UTC"
            };
            var existCode = await _context.Company.Find(_ => _.CompanyCode == company.CompanyCode).SingleOrDefaultAsync();
            if (existCode == null)
            {
                await _context.Company.InsertOneAsync(company);
                return company;
            }
            else
            {
                return new Company() { Id = "" };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Company> UpdateInfo(IUpdateCompany data)
        {
            var filter = Builders<Company>.Filter.Eq("Id", data.Id);
            var update = Builders<Company>.Update
                    .Set(s => s.CompanyName, data.CompanyName)
                    .Set(s => s.CompanyAddress, data.CompanyAddress)
                    .Set(s => s.Phone, data.Phone)
                    .Set(s => s.Fax, data.Fax)
                    .Set(s => s.Email, data.Email)
                    .Set(s => s.CompanyWebsite, data.CompanyWebsite)
                    .Set(s => s.CompanyType, data.CompanyType)
                    .Set(s => s.ScaleId, data.ScaleId)
                    .Set(s => s.TaxCode, data.TaxCode)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);

            var options = new FindOneAndUpdateOptions<Company>
            {
                ReturnDocument = ReturnDocument.After
            };
            var company = await _context.Company.FindOneAndUpdateAsync(filter, update, options);
            return company;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Company> UpdateLogo(IUploadLogoCompany data)
        {
            var filter = Builders<Company>.Filter.Eq("Id", data.Id);
            var update = Builders<Company>.Update
                        .Set(s => s.LogoUrl, data.LogoUrl)
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);

            var options = new FindOneAndUpdateOptions<Company>
            {
                ReturnDocument = ReturnDocument.After
            };
            var company = await _context.Company.FindOneAndUpdateAsync(filter, update, options);
            return company;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Company> UpdateMailSettings(IUpdateMailSettings data)
        {
            var filter = Builders<Company>.Filter.Eq("Id", data.Id);
            var update = Builders<Company>.Update
                    .Set(s => s.MailSettings, data.MailSettings)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);

            var options = new FindOneAndUpdateOptions<Company>
            {
                ReturnDocument = ReturnDocument.After
            };
            var company = await _context.Company.FindOneAndUpdateAsync(filter, update, options);
            return company;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Company> UpdateSettings(IUpdateSettingsCompany data)
        {
            var filter = Builders<Company>.Filter.Eq("Id", data.Id);
            var update = Builders<Company>.Update
                    .Set(s => s.OtherSettings, data.OtherSettings)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);

            var options = new FindOneAndUpdateOptions<Company>
            {
                ReturnDocument = ReturnDocument.After
            };
            var company = await _context.Company.FindOneAndUpdateAsync(filter, update, options);
            return company;
        }
    }
}
