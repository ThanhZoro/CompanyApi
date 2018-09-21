using System;
using System.Linq;
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
    public class ContactLeadRepository : IContactLeadRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        public ContactLeadRepository(ApplicationDbContext appcontext)
        {
           _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ContactLead> Create(ICreateContactLead data)
        {
            var contactLead = data.Cast<ContactLead>();
            contactLead.CreatedAt = DateTime.UtcNow;
            contactLead.IsDelete = false;
            await _context.ContactLead.InsertOneAsync(contactLead);
            return contactLead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ContactLead> Update(IUpdateContactLead data)
        {
            var filter = Builders<ContactLead>.Filter.Eq("Id", data.Id);
            var update = Builders<ContactLead>.Update
                    .Set(s => s.Name, data.Name)
                    .Set(s => s.Phone, data.Phone)
                    .Set(s => s.Email, data.Email)
                    .Set(s => s.Relationship, data.Relationship)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<ContactLead>
            {
                ReturnDocument = ReturnDocument.After
            };
            var contactLead = await _context.ContactLead.FindOneAndUpdateAsync(filter, update, options);
            return contactLead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ContactLead> Delete(IDeleteContactLead data)
        {
            var filter = Builders<ContactLead>.Filter.Eq("Id", data.Id);
            var update = Builders<ContactLead>.Update
                    .Set(s => s.IsDelete, true)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<ContactLead>
            {
                ReturnDocument = ReturnDocument.After
            };
            var contactLead = await _context.ContactLead.FindOneAndUpdateAsync(filter, update, options);
            return contactLead;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ContactLead> Upload(IUploadAvatarContactLead data)
        {
            var filter = Builders<ContactLead>.Filter.Eq("Id", data.Id);
            var update = Builders<ContactLead>.Update
                    .Set(s => s.AvatarUrl, data.AvatarUrl)
                    .Set(s => s.UpdatedBy, data.UpdatedBy)
                    .CurrentDate(s => s.UpdatedAt);
            var options = new FindOneAndUpdateOptions<ContactLead>
            {
                ReturnDocument = ReturnDocument.After
            };
            var contactLead = await _context.ContactLead.FindOneAndUpdateAsync(filter, update, options);
            return contactLead;
        }
    }
}
