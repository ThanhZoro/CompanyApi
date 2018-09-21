using Contracts.Commands;
using Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProductCategoryRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ProductCategory> Create(ICreateProductCategory data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<ProductCategory> Update(IUpdateProductCategory data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<ProductCategory>> Delete(IDeleteProductCategory data);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<string>> HasPermission(List<string> ids);
    }
}
