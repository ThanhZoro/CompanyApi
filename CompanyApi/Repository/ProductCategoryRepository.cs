using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiESReadService.Models;
using Contracts.Commands;
using Contracts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Nest;
using CompanyApi.Data;
using CompanyApi.Extensions;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductCategoryRepository : IProductCategoryRepository
    {
        private ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ElasticClient _esClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="esSettings"></param>
        public ProductCategoryRepository(
            ApplicationDbContext appcontext,
            IHttpContextAccessor httpContextAccessor,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _context = appcontext;
            _httpContextAccessor = httpContextAccessor;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("product_categories");
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ProductCategory> Create(ICreateProductCategory data)
        {
            var existCode = _context.ProductCategory.Find(f => f.CompanyId == data.CompanyId && f.IsDelete == false && f.CategoryCode == data.CategoryCode).FirstOrDefault();
            if (existCode == null)
            {
                var productCategory = data.Cast<ProductCategory>();
                productCategory.CreatedAt = DateTime.UtcNow;
                productCategory.IsDelete = false;
                await _context.ProductCategory.InsertOneAsync(productCategory);
                return productCategory;
            }
            return new ProductCategory() { Id = "-1" };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<ProductCategory>> Delete(IDeleteProductCategory data)
        {
            List<ProductCategory> productCategories = new List<ProductCategory>();
            foreach (var id in data.Ids)
            {
                var filter = Builders<ProductCategory>.Filter.Eq("Id", id);
                filter &= Builders<ProductCategory>.Filter.Eq("CompanyId", data.CompanyId);
                var update = Builders<ProductCategory>.Update
                            .Set(s => s.IsDelete, true)
                            .Set(s => s.UpdatedBy, data.UpdatedBy)
                            .CurrentDate(s => s.UpdatedAt);

                var options = new FindOneAndUpdateOptions<ProductCategory>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var productCategory = await _context.ProductCategory.FindOneAndUpdateAsync(filter, update, options);
                productCategories.Add(productCategory);
            }
            return productCategories;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ProductCategory> Update(IUpdateProductCategory data)
        {
            var existCode = _context.ProductCategory.Find(f => f.CompanyId == data.CompanyId && f.IsDelete == false && f.CategoryCode == data.CategoryCode && data.Id != f.Id).FirstOrDefault();
            if (existCode == null)
            {
                var filter = Builders<ProductCategory>.Filter.Eq("Id", data.Id);
                filter &= Builders<ProductCategory>.Filter.Eq("CompanyId", data.CompanyId);
                var update = Builders<ProductCategory>.Update
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);
                foreach (var column in data.Columns)
                {
                    switch(column)
                    {
                        case "CategoryCode":
                            update = update.Set(s => s.CategoryCode, data.CategoryCode);
                            break;
                        case "Name":
                            update = update.Set(s => s.Name, data.Name);
                            break;
                        case "ParentId":
                            update = update.Set(s => s.ParentId, data.ParentId);
                            break;
                        case "Status":
                            update = update.Set(s => s.Status, data.Status);
                            break;
                    }
                }

                var options = new FindOneAndUpdateOptions<ProductCategory>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var productCategory = await _context.ProductCategory.FindOneAndUpdateAsync(filter, update, options);
                return productCategory;
            }
            return new ProductCategory() { Id = "-1" };
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
            var userName = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(s => s.Type == "userName")?.Value;

            QueryContainer accessRightContainer = new QueryContainer();
            accessRightContainer &= Query<ProductCategory>.Ids(i => i.Values(ids));
            accessRightContainer &= Query<ProductCategory>.Term(t => t.Field(f => f.IsDelete).Value(false));
            accessRightContainer &= Query<ProductCategory>.Term(t => t.Field(f => f.CompanyId).Value(companyId));
            if (!roles.Contains("COMPANY_DATA"))
            {
                accessRightContainer &= Query<ProductCategory>.Term(t => t.CreatedBy, userName);
            }
            var searchResponse = await _esClient.SearchAsync<ProductCategory>(s => s
                       .Query(q => accessRightContainer));
            return searchResponse.Documents.Select(s => s.Id).ToList();
        }
    }
}
