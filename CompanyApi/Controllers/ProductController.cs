using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using O2OCRM.Authorization;
using CompanyApi.Models;
using CompanyApi.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyApi.Controllers
{
    /// <summary>
    /// summary for ProductController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class ProductController : Controller
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IRequestClient<ICreateProduct, Product> _createProductRequestClient;
        private readonly IRequestClient<IUpdateProduct, Product> _updateProductRequestClient;
        private readonly IRequestClient<IDeleteProduct, IProductDeleted> _deleteProductRequestClient;
        private readonly IProductRepository _productRepository;

        /// <summary>
        /// contructor ProductController
        /// </summary>
        /// <param name="createProductRequestClient"></param>
        /// <param name="updateProductRequestClient"></param>
        /// <param name="deleteProductRequestClient"></param>
        /// <param name="productRepository"></param>
        public ProductController(
            IRequestClient<ICreateProduct, Product> createProductRequestClient,
            IRequestClient<IUpdateProduct, Product> updateProductRequestClient,
            IRequestClient<IDeleteProduct, IProductDeleted> deleteProductRequestClient,
            IProductRepository productRepository)
        {
            _createProductRequestClient = createProductRequestClient;
            _updateProductRequestClient = updateProductRequestClient;
            _deleteProductRequestClient = deleteProductRequestClient;
            _productRepository = productRepository;
        }

        /// <summary>
        /// create product
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new  product</returns>
        /// <response code="200">returns the newly created product</response>
        [HttpPut]
        [ProducesResponseType(typeof(Product), 200)]
        [AccessRight("PRODUCT_EDIT")]
        public async Task<IActionResult> Create([FromBody]CreateProduct data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _createProductRequestClient.Request(data);
                if (result.Id != "-1")
                    return Ok(result);
                else
                    return BadRequest("productCodeExisted");
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update product
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the updated product</returns>
        /// <response code="200">returns the updated product</response>
        [HttpPost]
        [ProducesResponseType(typeof(Product), 200)]
        [AccessRight("PRODUCT_EDIT")]
        public async Task<IActionResult> Update([FromBody]UpdateProduct data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                var idsHasPermission = await _productRepository.HasPermission(new List<string>() { data.Id });
                if (idsHasPermission.Count == 0)
                {
                    return Unauthorized();
                }
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _updateProductRequestClient.Request(data);
                if (result.Id != "-1")
                    return Ok(result);
                else
                    return BadRequest("productCodeExisted");
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete products
        /// </summary>
        /// <param name="data">data info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the deleted products</returns>
        /// <response code="200">returns the deleted products</response>
        [HttpDelete]
        [ProducesResponseType(typeof(IProductDeleted), 200)]
        [AccessRight("PRODUCT_DELETE")]
        public async Task<IActionResult> Delete([FromBody]DeleteProduct data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                var idsHasPermission = await _productRepository.HasPermission(new List<string>(data.Ids));
                if (idsHasPermission.Count == 0)
                {
                    return Unauthorized();
                }
                data.Ids = idsHasPermission;
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _deleteProductRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
    }
}