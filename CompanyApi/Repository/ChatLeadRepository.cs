using Contracts.Commands;
using Contracts.Models;
using CompanyApi.Data;
using CompanyApi.Extensions;
using System;
using System.Threading.Tasks;

namespace CompanyApi.Repository
{
    /// <summary>
    /// 
    /// </summary>
    public class ChatLeadRepository : IChatLeadRepository
    {
        private ApplicationDbContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appcontext"></param>
        public ChatLeadRepository(ApplicationDbContext appcontext)
        {
            _context = appcontext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<ChatLead> Create(ICreateChatLead data)
        {
            var chatLead = data.Cast<ChatLead>();
            chatLead.CreatedAt = DateTime.UtcNow;
            await _context.ChatLead.InsertOneAsync(chatLead);
            return chatLead;
        }
    }
}
