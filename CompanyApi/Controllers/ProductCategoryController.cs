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
    /// summary for ProductCategoryController
    /// </summary>
    [Authorize]
    [Route("api/company/[controller]/[action]")]
    public class ProductCategoryController : Controller
    {
        private readonly IRequestClient<ICreateProductCategory, ProductCategory> _createProductCategoryRequestClient;
        private readonly IRequestClient<IUpdateProductCategory, ProductCategory> _updateProductCategoryRequestClient;
        private readonly IRequestClient<IDeleteProductCategory, IProductCategoryDeleted> _deleteProductCategoryRequestClient;
        private readonly IProductCategoryRepository _productCategoryRepository;

        /// <summary>
        /// contructor ProductCategoryController
        /// </summary>
        /// <param name="createProductCategoryRequestClient"></param>
        /// <param name="updateProductCategoryRequestClient"></param>
        /// <param name="deleteProductCategoryRequestClient"></param>
        /// <param name="productCategoryRepository"></param>
        public ProductCategoryController(
            IRequestClient<ICreateProductCategory, ProductCategory> createProductCategoryRequestClient,
            IRequestClient<IUpdateProductCategory, ProductCategory> updateProductCategoryRequestClient,
            IRequestClient<IDeleteProductCategory, IProductCategoryDeleted> deleteProductCategoryRequestClient,
            IProductCategoryRepository productCategoryRepository)
        {
            _createProductCategoryRequestClient = createProductCategoryRequestClient;
            _updateProductCategoryRequestClient = updateProductCategoryRequestClient;
            _deleteProductCategoryRequestClient = deleteProductCategoryRequestClient;
            _productCategoryRepository = productCategoryRepository;
        }

        /// <summary>
        /// create product category
        /// </summary>
        /// <param name="data">create info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>a new  product category</returns>
        /// <response code="200">returns the newly created product category</response>
        [HttpPut]
        [ProducesResponseType(typeof(ProductCategory), 200)]
        [AccessRight("PRODUCT_EDIT")]
        public async Task<IActionResult> Create([FromBody]CreateProductCategory data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                data.CreatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _createProductCategoryRequestClient.Request(data);
                if (result.Id != "-1")
                    return Ok(result);
                else
                    return BadRequest("categoryCodeExisted");
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// update product category
        /// </summary>
        /// <param name="data">update info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the updated product category</returns>
        /// <response code="200">returns the updated product category</response>
        [HttpPost]
        [ProducesResponseType(typeof(ProductCategory), 200)]
        [AccessRight("PRODUCT_EDIT")]
        public async Task<IActionResult> Update([FromBody]UpdateProductCategory data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                var idsHasPermission = await _productCategoryRepository.HasPermission(new List<string>() { data.Id });
                if (idsHasPermission.Count == 0)
                {
                    return Unauthorized();
                }
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _updateProductCategoryRequestClient.Request(data);
                if (result.Id != "-1")
                    return Ok(result);
                else
                    return BadRequest("categoryCodeExisted");
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// delete product categories
        /// </summary>
        /// <param name="data">data info</param>
        /// <param name="CompanyId">company id from header</param>
        /// <returns>the deleted product categories</returns>
        /// <response code="200">returns the deleted product categories</response>
        [HttpDelete]
        [ProducesResponseType(typeof(IProductCategoryDeleted), 200)]
        [AccessRight("PRODUCT_DELETE")]
        public async Task<IActionResult> Delete([FromBody]DeleteProductCategory data, [FromHeader]string CompanyId)
        {
            if (ModelState.IsValid)
            {
                var idsHasPermission = await _productCategoryRepository.HasPermission(new List<string>(data.Ids));
                if (idsHasPermission.Count == 0)
                {
                    return Unauthorized();
                }
                data.Ids = idsHasPermission;
                data.UpdatedBy = User.Claims.FirstOrDefault(s => s.Type == "userName").Value;
                data.CompanyId = CompanyId;
                var result = await _deleteProductCategoryRequestClient.Request(data);
                return Ok(result);
            }
            return BadRequest(ModelState);
        }
    }
}