using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Contracts.Commands;
using Contracts.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using CompanyApi.Data;
using CompanyApi.Extensions;
using CompanyApi.Models;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class CommonDataRepository : ICommonDataRepository
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public CommonDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<CommonData> Create(ICreateCommonData data)
        {
            var exist = await _context.CommonData.Find(s => s.CompanyId == data.CompanyId && s.DataKey == data.DataKey).FirstOrDefaultAsync();
            if (exist != null)
            {
                if (exist.IsDelete)
                {
                    var filter = Builders<CommonData>.Filter.Eq("Id", exist.Id);
                    var update = Builders<CommonData>.Update
                                .Set(s => s.DataType, data.DataType)
                                .Set(s => s.Color, data.Color)
                                .Set(s => s.Weight, data.Weight)
                                .Set(s => s.DataValue, data.DataValue)
                                .Set(s => s.IsDelete, false)
                                .Set(s => s.UpdatedBy, data.CreatedBy)
                                .CurrentDate(s => s.UpdatedAt);
                    var options = new FindOneAndUpdateOptions<CommonData>
                    {
                        ReturnDocument = ReturnDocument.After
                    };
                    var result = await _context.CommonData.FindOneAndUpdateAsync(filter, update, options);
                    return result;

                }
                else
                {
                    throw new Exception("keyExisted");
                }
            }
            else
            {
                var commonData = data.Cast<CommonData>();
                commonData.CreatedAt = DateTime.UtcNow;
                await _context.CommonData.InsertOneAsync(commonData);
                return commonData;
            }
        }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="data"></param>
       /// <returns></returns>
        public async Task<List<CommonData>> CreateDefaultCommonData(ICreateDefaultCommonData data)
        {
            List<DefaultCommonData> commonDatas = new List<DefaultCommonData>();
            List<CommonData> datas = new List<CommonData>();
            var path = data.Culture == "vi" ? "./wwwroot/DefaultData/default_common_data_vi.json" : "./wwwroot/DefaultData/default_common_data_en.json";
            using (StreamReader r = new StreamReader(path))
            {
                string json = r.ReadToEnd();
                commonDatas = JsonConvert.DeserializeObject<List<DefaultCommonData>>(json);
            }
            foreach (var item in commonDatas)
            {
                var common = new CommonData()
                {
                    CompanyId = data.CompanyId,
                    DataType = (CommonDataType)item.DataType,
                    DataKey = item.DataKey,
                    DataValue = item.DataValue,
                    Color = item.Color,
                    Weight = item.Weight,
                    IsDelete = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = data.CreatedBy
                };
                await _context.CommonData.InsertOneAsync(common);
                datas.Add(common);
            }
            return datas;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<CommonData> Delete(IDeleteCommonData data)
        {
            var filter = Builders<CommonData>.Filter.Eq("Id", data.Id);
            var update = Builders<CommonData>.Update
                        .Set(s => s.IsDelete, true)
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<CommonData>
            {
                ReturnDocument = ReturnDocument.After
            };
            var result = await _context.CommonData.FindOneAndUpdateAsync(filter, update, options);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Task<List<CommonData>> GetAll(string companyId)
        {
            var data = _context.CommonData.Find(s => s.CompanyId == companyId).ToListAsync();
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<CommonData> Update(IUpdateCommonData data)
        {
            var filter = Builders<CommonData>.Filter.Eq("Id", data.Id);
            var update = Builders<CommonData>.Update
                        .Set(s => s.DataType, data.DataType)
                        .Set(s => s.Color, data.Color)
                        .Set(s => s.Weight, data.Weight)
                        .Set(s => s.DataValue, data.DataValue)
                        .Set(s => s.IsDelete, false)
                        .Set(s => s.UpdatedBy, data.UpdatedBy)
                        .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<CommonData>
            {
                ReturnDocument = ReturnDocument.After
            };
            var result = await _context.CommonData.FindOneAndUpdateAsync(filter, update, options);
            return result;
        }
    }
}
