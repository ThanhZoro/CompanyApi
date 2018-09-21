using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ApiESReadService.Models;
using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nest;
using CompanyApi.Data;
using CompanyApi.Extensions;
using CompanyApi.Models;
using OfficeOpenXml;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class LeadRepository : ILeadRepository
    {
        private readonly ICommonDataRepository _commonDataRepository;
        private ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commonDataRepository"></param>
        /// <param name="appcontext"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="esSettings"></param>
        public LeadRepository(
            ICommonDataRepository commonDataRepository,
            ApplicationDbContext appcontext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _context = appcontext;
            _commonDataRepository = commonDataRepository;
            _httpContextAccessor = httpContextAccessor;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("leads");
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<LeadEdited> Create(ICreateLead data)
        {
            var lead = data.Cast<Lead>();
            var validate = await ValidateLeadRule(lead);
            if (validate.LeadRequire.Count > 0 || validate.LeadDuplicate.Count > 0)
            {
                return new LeadEdited()
                {
                    IsSuccess = false,
                    DataFail = new EditLeadFail()
                    {
                        LeadDuplicate = validate.LeadDuplicate,
                        LeadRequire = validate.LeadRequire
                    }
                };
            }
            else
            {
                lead.CreatedAt = DateTime.UtcNow;
                await _context.Lead.InsertOneAsync(lead);
                return new LeadEdited()
                {
                    IsSuccess = true,
                    DataSuccess = lead
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<LeadEdited> Update(IUpdateLead data)
        {
            var leadCreate = data.Cast<Lead>();
            var validate = await ValidateLeadRule(leadCreate, true, data.Columns);
            if (validate.LeadRequire.Count > 0 || validate.LeadDuplicate.Count > 0)
            {
                return new LeadEdited()
                {
                    IsSuccess = false,
                    DataFail = new EditLeadFail()
                    {
                        LeadDuplicate = validate.LeadDuplicate,
                        LeadRequire = validate.LeadRequire
                    }
                };
            }
            else
            {
                var filter = Builders<Lead>.Filter.Eq("Id", data.Id);
                var update = Builders<Lead>.Update
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);
                foreach (var column in data.Columns)
                {
                    switch (column)
                    {
                        case "FullName":
                            update = update.Set(s => s.FullName, data.FullName);
                            break;
                        case "Vocative":
                            update = update.Set(s => s.Vocative, data.Vocative);
                            break;
                        case "Phone":
                            update = update.Set(s => s.Phone, data.Phone);
                            break;
                        case "Email":
                            update = update.Set(s => s.Email, data.Email);
                            break;
                        case "Status":
                            update = update.Set(s => s.Status, data.Status);
                            break;
                        case "Source":
                            update = update.Set(s => s.Source, data.Source);
                            break;
                        case "Interest":
                            update = update.Set(s => s.Interest, data.Interest);
                            break;
                        case "Note":
                            update = update.Set(s => s.Note, data.Note);
                            break;
                        case "SupportStaff":
                            update = update.Set(s => s.SupportStaff, data.SupportStaff);
                            break;
                        case "Gender":
                            update = update.Set(s => s.Gender, data.Gender);
                            break;
                        case "Birthday":
                            update = update.Set(s => s.Birthday, data.Birthday);
                            break;
                        case "Address":
                            update = update.Set(s => s.Address, data.Address);
                            break;
                        case "IdentityCard":
                            update = update.Set(s => s.IdentityCard, data.IdentityCard);
                            break;
                        case "DateOfIssue":
                            update = update.Set(s => s.DateOfIssue, data.DateOfIssue);
                            break;
                        case "PlaceOfIssue":
                            update = update.Set(s => s.PlaceOfIssue, data.PlaceOfIssue);
                            break;
                        case "MaritalStatus":
                            update = update.Set(s => s.MaritalStatus, data.MaritalStatus);
                            break;
                        case "Relationship":
                            update = update.Set(s => s.Relationship, data.Relationship);
                            break;
                        case "SocialId":
                            update = update.Set(s => s.SocialId, data.SocialId);
                            break;
                        case "Channel":
                            update = update.Set(s => s.Channel, data.Channel);
                            break;
                        case "Campaign":
                            update = update.Set(s => s.Campaign, data.Campaign);
                            break;
                    }
                }
                var options = new FindOneAndUpdateOptions<Lead>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var lead = await _context.Lead.FindOneAndUpdateAsync(filter, update, options);
                return new LeadEdited()
                {
                    IsSuccess = true,
                    DataSuccess = lead
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<Lead>> Delete(IDeleteLead data)
        {
            List<Lead> leads = new List<Lead>();
            foreach (var id in data.Ids)
            {
                var filter = Builders<Lead>.Filter.Eq("Id", id);
                var update = Builders<Lead>.Update
                            .Set(s => s.IsDelete, true)
                            .Set(s => s.UpdatedBy, data.UpdatedBy)
                            .CurrentDate(s => s.UpdatedAt);

                var options = new FindOneAndUpdateOptions<Lead>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var lead = await _context.Lead.FindOneAndUpdateAsync(filter, update, options);
                leads.Add(lead);
            }
            return leads;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<Lead>> ImportFromExcel(IImportLead data)
        {
            var leads = await FilterDataByRules(data.ListLeads);
            var listLeads = new List<Lead>();
            foreach (var lead in leads)
            {
                var item = lead.Cast<Lead>();
                item.CreatedAt = DateTime.UtcNow;
                item.IsDelete = false;
                listLeads.Add(item);
            }

            if (listLeads.Count > 0)
            {
                await _context.Lead.InsertManyAsync(listLeads);
            }
            return listLeads;
        }

        async Task<List<ICreateLead>> FilterDataByRules(List<ICreateLead> data)
        {
            var companyId = data.FirstOrDefault()?.CompanyId;
            var company = await _context.Company.Find(f => f.Id == companyId).FirstOrDefaultAsync();
            if (company != null)
            {
                //validate require
                Expression<Func<ICreateLead, bool>> expressionAnd = n => true;
                foreach (var item in company.OtherSettings?.LeadRequire)
                {
                    switch (item.ToLower())
                    {
                        case "fullname":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.FullName));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "phone":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Phone));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "email":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Email));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "gender":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Gender));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "birthday":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => e.Birthday != null);
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "adrress":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Address));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "status":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Status));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "channel":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Channel));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                        case "source":
                            {
                                Expression<Func<ICreateLead, bool>> expression = (e => !string.IsNullOrEmpty(e.Source));
                                expressionAnd = CombineWithAnd(expressionAnd, expression);
                                break;
                            }
                    }
                }
                data = data.Where(expressionAnd.Compile()).ToList();

                //validate duplicate
                var leads = _context.Lead.AsQueryable().Where(w => w.CompanyId == companyId && w.IsDelete == false).ToList();
                Expression<Func<ICreateLead, bool>> resultExpression = n => false;

                foreach (var item in company.OtherSettings?.LeadDuplicate)
                {
                    if (item.ToLower() == "fullname")
                    {
                        Expression<Func<ICreateLead, bool>> expression = (e => !leads.Any(a => a.FullName == e.FullName && !string.IsNullOrEmpty(e.FullName)));
                        resultExpression = CombineWithOr(resultExpression, expression);
                    }
                    if (item.ToLower() == "phone")
                    {
                        Expression<Func<ICreateLead, bool>> expression = (e => !leads.Any(a => a.Phone == e.Phone && !string.IsNullOrEmpty(e.Phone)));
                        resultExpression = CombineWithOr(resultExpression, expression);
                    }
                    if (item.ToLower() == "email")
                    {
                        Expression<Func<ICreateLead, bool>> expression = (e => !leads.Any(a => a.Email == e.Email && !string.IsNullOrEmpty(e.Email)));
                        resultExpression = CombineWithOr(resultExpression, expression);
                    }
                    if (item.ToLower() == "identitycard")
                    {
                        Expression<Func<ICreateLead, bool>> expression = (e => !leads.Any(a => a.IdentityCard == e.IdentityCard && !string.IsNullOrEmpty(e.IdentityCard)));
                        resultExpression = CombineWithOr(resultExpression, expression);
                    }
                    if (item.ToLower() == "socialid")
                    {
                        Expression<Func<ICreateLead, bool>> expression = (e => !leads.Any(a => a.SocialId == e.SocialId && !string.IsNullOrEmpty(e.SocialId)));
                        resultExpression = CombineWithOr(resultExpression, expression);
                    }
                }
                if (company.OtherSettings?.LeadDuplicate.Count > 0)
                    data = data.Where(resultExpression.Compile()).ToList();
                return data;
            }
            return new List<ICreateLead>();
        }

        Expression<Func<T, bool>> CombineWithOr<T>(Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
        {
            // Create a parameter to use for both of the expression bodies.
            var parameter = Expression.Parameter(typeof(T), "x");
            // Invoke each expression with the new parameter, and combine the expression bodies with OR.
            var resultBody = Expression.Or(Expression.Invoke(firstExpression, parameter), Expression.Invoke(secondExpression, parameter));
            // Combine the parameter with the resulting expression body to create a new lambda expression.
            return Expression.Lambda<Func<T, bool>>(resultBody, parameter);
        }

        Expression<Func<T, bool>> CombineWithAnd<T>(Expression<Func<T, bool>> firstExpression, Expression<Func<T, bool>> secondExpression)
        {
            // Create a parameter to use for both of the expression bodies.
            var parameter = Expression.Parameter(typeof(T), "x");
            // Invoke each expression with the new parameter, and combine the expression bodies with AND.
            var resultBody = Expression.And(Expression.Invoke(firstExpression, parameter), Expression.Invoke(secondExpression, parameter));
            // Combine the parameter with the resulting expression body to create a new lambda expression.
            return Expression.Lambda<Func<T, bool>>(resultBody, parameter);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<string> Download(string filename, string companyId)
        {
            var sourcePath = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/ImportTemplate", filename);
            var targetPath = Path.Combine(
                           Directory.GetCurrentDirectory(),
                           "wwwroot/ImportTemplateData", $"{Path.GetFileNameWithoutExtension(filename)}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.{Path.GetExtension(filename)}");
            File.Copy(sourcePath, targetPath, true);
            FileInfo file = new FileInfo(targetPath);
            using (ExcelPackage excelPackage = new ExcelPackage(file))
            {
                ExcelWorkbook excelWorkBook = excelPackage.Workbook;
                ExcelWorksheet excelWorksheet = excelWorkBook.Worksheets["Master"];

                //insert lead status for excel
                int row = 2;
                int column = 1;
                var commonData = await _commonDataRepository.GetAll(companyId);
                var leadStatus = commonData.Where(w => w.DataType == CommonDataType.Status && w.IsDelete == false);
                foreach (var status in leadStatus)
                {
                    excelWorksheet.Cells[row, column].Value = $"{status.DataKey}-{status.DataValue}";
                    row++;
                }

                //insert defintion source for excel
                row = 2;
                column = 7;
                var definitionSources = commonData.Where(w => w.DataType == CommonDataType.Source && w.IsDelete == false);
                foreach (var source in definitionSources)
                {
                    excelWorksheet.Cells[row, column].Value = $"{source.DataKey}-{source.DataValue}";
                    row++;
                }

                //insert definition channel for excel
                row = 2;
                column = 9;
                var definitionChannels = commonData.Where(w => w.DataType == CommonDataType.Channel && w.IsDelete == false);
                foreach (var channel in definitionChannels)
                {
                    excelWorksheet.Cells[row, column].Value = $"{channel.DataKey}-{channel.DataValue}";
                    row++;
                }

                //insert gender
                row = 2;
                column = 3;
                var genders = commonData.Where(w => w.DataType == CommonDataType.Gender && w.IsDelete == false);
                foreach (var gender in genders)
                {
                    excelWorksheet.Cells[row, column].Value = $"{gender.DataKey}-{gender.DataValue}";
                    row++;
                }

                //insert vocative
                row = 2;
                column = 5;
                var vocatives = commonData.Where(w => w.DataType == CommonDataType.Vocative && w.IsDelete == false);
                foreach (var vocative in vocatives)
                {
                    excelWorksheet.Cells[row, column].Value = $"{vocative.DataKey}-{vocative.DataValue}";
                    row++;
                }

                //insert Marital Status
                row = 2;
                column = 11;
                var maritalStatus = commonData.Where(w => w.DataType == CommonDataType.MaritalStatus && w.IsDelete == false);
                foreach (var status in maritalStatus)
                {
                    excelWorksheet.Cells[row, column].Value = $"{status.DataKey}-{status.DataValue}";
                    row++;
                }

                //insert Relationship
                row = 2;
                column = 13;
                var relationships = commonData.Where(w => w.DataType == CommonDataType.Relationship && w.IsDelete == false);
                foreach (var relationship in relationships)
                {
                    excelWorksheet.Cells[row, column].Value = $"{relationship.DataKey}-{relationship.DataValue}";
                    row++;
                }

                excelPackage.Save();
            }
            return targetPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Lead> AddSupportStaff(IAddSupportStaff data)
        {
            var filter = Builders<Lead>.Filter.Eq("Id", data.Id);
            var update = Builders<Lead>.Update
                    .Push(s => s.SupportStaff, data.SupportStaffId)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<Lead>
            {
                ReturnDocument = ReturnDocument.After
            };
            var lead = await _context.Lead.FindOneAndUpdateAsync(filter, update, options);
            return lead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Lead> RemoveSupportStaff(IRemoveSupportStaff data)
        {
            var filter = Builders<Lead>.Filter.Eq("Id", data.Id);
            var update = Builders<Lead>.Update
                    .Pull(s => s.SupportStaff, data.SupportStaffId)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<Lead>
            {
                ReturnDocument = ReturnDocument.After
            };
            var lead = await _context.Lead.FindOneAndUpdateAsync(filter, update, options);
            return lead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lead"></param>
        /// <param name="isUpdate"></param>
        /// <param name="columns"></param>
        /// <returns></returns>
        async Task<ValidateLeadRule> ValidateLeadRule(Lead lead, bool isUpdate = false, List<string> columns = null)
        {
            ValidateLeadRule validate = new ValidateLeadRule();
            List<string> listLeadDuplicate = new List<string>();
            List<string> listLeadRequire = new List<string>();
            var company = await _context.Company.Find(f => f.Id == lead.CompanyId).FirstOrDefaultAsync();
            //validate duplicate
            if (company.OtherSettings.LeadDuplicate.Count > 0)
            {
                var filter = Builders<Lead>.Filter.Eq(f => f.CompanyId, lead.CompanyId);
                filter &= Builders<Lead>.Filter.Eq(f => f.IsDelete, false);
                var or2 = new List<FilterDefinition<Lead>>();
                foreach (var item in company.OtherSettings.LeadDuplicate)
                {
                    if (item.ToLower() == "fullname" && !string.IsNullOrEmpty(lead.FullName))
                    {
                        or2.Add(Builders<Lead>.Filter.Eq(f => f.FullName, lead.FullName));
                    }
                    if (item.ToLower() == "phone" && !string.IsNullOrEmpty(lead.Phone))
                    {
                        or2.Add(Builders<Lead>.Filter.Eq(f => f.Phone, lead.Phone));
                    }
                    if (item.ToLower() == "email" && !string.IsNullOrEmpty(lead.Email))
                    {
                        or2.Add(Builders<Lead>.Filter.Eq(f => f.Email, lead.Email));
                    }
                    if (item.ToLower() == "identitycard" && !string.IsNullOrEmpty(lead.IdentityCard))
                    {
                        or2.Add(Builders<Lead>.Filter.Eq(f => f.IdentityCard, lead.IdentityCard));
                    }
                    if (item.ToLower() == "socialid" && !string.IsNullOrEmpty(lead.SocialId))
                    {
                        or2.Add(Builders<Lead>.Filter.Eq(f => f.SocialId, lead.SocialId));
                    }
                }
                if (or2.Count > 0)
                    filter &= Builders<Lead>.Filter.Or(or2);
                if (!string.IsNullOrEmpty(lead.Id))
                {
                    filter &= !Builders<Lead>.Filter.Eq(f => f.Id, lead.Id);
                }
                var result = await _context.Lead.FindAsync(filter);
                var leads = result.ToList();
                foreach (var item in company.OtherSettings.LeadDuplicate)
                {
                    switch (item.ToLower())
                    {
                        case "fullName":
                            var existName = leads.Any(a => a.FullName == lead.FullName && !string.IsNullOrEmpty(lead.FullName));
                            if (existName)
                                listLeadDuplicate.Add("fullName");
                            break;
                        case "phone":
                            var existPhone = leads.Any(a => a.Phone == lead.Phone && !string.IsNullOrEmpty(lead.Phone));
                            if (existPhone)
                                listLeadDuplicate.Add("phone");
                            break;
                        case "email":
                            var exisEmail = leads.Any(a => a.Email == lead.Email && !string.IsNullOrEmpty(lead.Email));
                            if (exisEmail)
                                listLeadDuplicate.Add("email");
                            break;
                        case "identityCard":
                            var exisIdentityCard = leads.Any(a => a.IdentityCard == lead.IdentityCard && !string.IsNullOrEmpty(lead.IdentityCard));
                            if (exisIdentityCard)
                                listLeadDuplicate.Add("identityCard");
                            break;
                        case "socialId":
                            var existSocialId = leads.Any(a => a.SocialId == lead.SocialId && !string.IsNullOrEmpty(lead.SocialId));
                            if (existSocialId)
                                listLeadDuplicate.Add("socialId");
                            break;
                    }
                }
                validate.LeadDuplicate = listLeadDuplicate.Distinct().ToList();

                //validate require
                if (company.OtherSettings.LeadRequire.Count > 0)
                {
                    foreach (var item in company.OtherSettings.LeadRequire)
                    {
                        switch (item.ToLower())
                        {
                            case "fullname":
                                {
                                    if (string.IsNullOrEmpty(lead.FullName))
                                        listLeadRequire.Add("fullName");
                                    break;
                                }
                            case "phone":
                                {
                                    if (string.IsNullOrEmpty(lead.Phone))
                                        listLeadRequire.Add("phone");
                                    break;
                                }
                            case "email":
                                {
                                    if (string.IsNullOrEmpty(lead.Email))
                                        listLeadRequire.Add("email");
                                    break;
                                }
                            case "gender":
                                {
                                    if (string.IsNullOrEmpty(lead.Gender))
                                        listLeadRequire.Add("gender");
                                    break;
                                }
                            case "birthday":
                                {
                                    if (lead.Birthday == null)
                                        listLeadRequire.Add("birthday");
                                    break;
                                }
                            case "adrress":
                                {
                                    if (string.IsNullOrEmpty(lead.Address))
                                        listLeadRequire.Add("adrress");
                                    break;
                                }
                            case "status":
                                {
                                    if (string.IsNullOrEmpty(lead.Status))
                                        listLeadRequire.Add("status");
                                    break;
                                }
                            case "channel":
                                {
                                    if (string.IsNullOrEmpty(lead.Channel))
                                        listLeadRequire.Add("channel");
                                    break;
                                }
                            case "source":
                                {
                                    if (string.IsNullOrEmpty(lead.Source))
                                        listLeadRequire.Add("source");
                                    break;
                                }
                        }
                    }
                    validate.LeadRequire = listLeadRequire;
                }

            }
            if (isUpdate)
            {
                var col = columns.Select(s => s.ToLower());
                validate.LeadRequire = validate.LeadRequire.Intersect(columns).ToList();
            }
            return validate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<Lead>> AddStaffInCharge(IAddStaffInCharge data)
        {
            List<Lead> leads = new List<Lead>();
            List<Lead> updateLeads = _context.Lead.AsQueryable().Where(w => data.Ids.Contains(w.Id) && (data.StaffInCharge != w.StaffInCharge || data.TeamId != w.TeamId)).ToList();
            foreach (var updateLead in updateLeads)
            {
                var filter = Builders<Lead>.Filter.Eq("Id", updateLead.Id);
                filter &= Builders<Lead>.Filter.Eq("CompanyId", data.CompanyId);
                var update = Builders<Lead>.Update
                        .Set(s => s.StaffInCharge, data.StaffInCharge)
                        .Set(s => s.TeamId, data.TeamId)
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);
                var options = new FindOneAndUpdateOptions<Lead>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var lead = await _context.Lead.FindOneAndUpdateAsync(filter, update, options);
                leads.Add(lead);
            }
            return leads;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<List<string>> HasPermission(List<string> ids)
        {
            var roles = _httpContextAccessor.HttpContext.GetRouteValue("roles")?.ToString().Split(",");
            var companyId = _httpContextAccessor.HttpContext.Request?.Headers["CompanyId"].FirstOrDefault();
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "sub")?.Value;
            var teams = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "teams")?.Value;

            QueryContainer accessRightContainer = new QueryContainer();
            accessRightContainer &= Query<Lead>.Ids(i => i.Values(ids));
            accessRightContainer &= Query<Lead>.Term(t => t.Field(f => f.IsDelete).Value(false));
            accessRightContainer &= Query<Lead>.Term(t => t.Field(f => f.CompanyId).Value(companyId));
            if (!roles.Contains("COMPANY_DATA"))
            {
                if (!roles.Contains("TEAM_DATA_EDIT"))
                    accessRightContainer &= Query<Lead>.Term(t => t.StaffInCharge, userId);
                else
                    accessRightContainer &= Query<Lead>.Terms(t => t.Field(f => f.TeamId).Terms(teams));
            }
            var searchResponse = await _esClient.SearchAsync<Lead>(s => s
                       .Query(q => accessRightContainer));
            return searchResponse.Documents.Select(s => s.Id).ToList();
        }
    }
}
