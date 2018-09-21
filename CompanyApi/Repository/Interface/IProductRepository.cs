using Contracts.Commands;
using Contracts.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProductRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Product> Create(ICreateProduct data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<Product> Update(IUpdateProduct data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<List<Product>> Delete(IDeleteProduct data);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<List<string>> HasPermission(List<string> ids);
    }
}
