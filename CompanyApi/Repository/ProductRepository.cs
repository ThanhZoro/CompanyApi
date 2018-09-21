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
    public class ProductRepository : IProductRepository
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
        public ProductRepository(
            ApplicationDbContext appcontext, 
            IHttpContextAccessor httpContextAccessor,
            IOptions<ElasticSearchSettings> esSettings)
        {
            _context = appcontext;
            _httpContextAccessor = httpContextAccessor;
            var node = new Uri($"http://{esSettings.Value.Host}:{esSettings.Value.Port}");
            var connSettings = new ConnectionSettings(node);
            connSettings.DefaultIndex("products");
            _esClient = new ElasticClient(connSettings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Product> Create(ICreateProduct data)
        {
            var existCode = _context.Product.Find(f => f.CompanyId == data.CompanyId && f.IsDelete == false && f.ProductCode == data.ProductCode).FirstOrDefault();
            if (existCode == null)
            {
                var product = data.Cast<Product>();
                product.CreatedAt = DateTime.UtcNow;
                product.IsDelete = false;
                await _context.Product.InsertOneAsync(product);
                return product;
            }
            return new Product() { Id = "-1" };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<Product>> Delete(IDeleteProduct data)
        {
            List<Product> products = new List<Product>();
            foreach (var id in data.Ids)
            {
                var filter = Builders<Product>.Filter.Eq("Id", id);
                filter &= Builders<Product>.Filter.Eq("CompanyId", data.CompanyId);
                var update = Builders<Product>.Update
                            .Set(s => s.IsDelete, true)
                            .Set(s => s.UpdatedBy, data.UpdatedBy)
                            .CurrentDate(s => s.UpdatedAt);

                var options = new FindOneAndUpdateOptions<Product>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var product = await _context.Product.FindOneAndUpdateAsync(filter, update, options);
                products.Add(product);
            }
            return products;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<Product> Update(IUpdateProduct data)
        {
            var existCode = _context.Product.Find(f => f.CompanyId == data.CompanyId && f.IsDelete == false && f.ProductCode == data.ProductCode && data.Id != f.Id).FirstOrDefault();
            if (existCode == null)
            {
                var filter = Builders<Product>.Filter.Eq("Id", data.Id);
                filter &= Builders<Product>.Filter.Eq("CompanyId", data.CompanyId);
                var update = Builders<Product>.Update
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);
                foreach(var column in data.Columns)
                {
                    switch(column)
                    {
                        case "ProductCode":
                            update = update.Set(s => s.ProductCode, data.ProductCode);
                            break;
                        case "ProductCategoryId":
                            update = update.Set(s => s.ProductCategoryId, data.ProductCategoryId);
                            break;
                        case "Name":
                            update = update.Set(s => s.Name, data.Name);
                            break;
                        case "Unit":
                            update = update.Set(s => s.Unit, data.Unit);
                            break;
                        case "Price":
                            update = update.Set(s => s.Price, data.Price);
                            break;
                        case "Desc":
                            update = update.Set(s => s.Desc, data.Desc);
                            break;
                        case "Status":
                            update = update.Set(s => s.Status, data.Status);
                            break;
                    }
                }

                var options = new FindOneAndUpdateOptions<Product>
                {
                    ReturnDocument = ReturnDocument.After
                };
                var product = await _context.Product.FindOneAndUpdateAsync(filter, update, options);
                return product;
            }
            return new Product() { Id = "-1" };
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
            accessRightContainer &= Query<Product>.Ids(i => i.Values(ids));
            accessRightContainer &= Query<Product>.Term(t => t.Field(f => f.IsDelete).Value(false));
            accessRightContainer &= Query<Product>.Term(t => t.Field(f => f.CompanyId).Value(companyId));
            if (!roles.Contains("COMPANY_DATA"))
            {
                accessRightContainer &= Query<Product>.Term(t => t.CreatedBy, userName);
            }
            var searchResponse = await _esClient.SearchAsync<Product>(s => s
                       .Query(q => accessRightContainer));
            return searchResponse.Documents.Select(s => s.Id).ToList();
        }
    }
}
