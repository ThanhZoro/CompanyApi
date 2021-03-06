﻿using Contracts.Models;
using MongoDB.Driver;
using System;

namespace CompanyApi.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database = null;
        private readonly IMongoDatabase _databaseNotification = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="database"></param>
        public ApplicationDbContext(string connectionString, string database)
        {
            var client = new MongoClient(connectionString);
            if (client != null)
                _database = client.GetDatabase(database);
            var clientNotification = new MongoClient($"mongodb://{Environment.GetEnvironmentVariable("MONGODB_USERNAME")}:{Environment.GetEnvironmentVariable("MONGODB_PASSWORD")}@{Environment.GetEnvironmentVariable("NOTIFICATION_MONGODB_HOST")}:{Environment.GetEnvironmentVariable("NOTIFICATION_MONGODB_PORT")}");
            if (clientNotification != null)
                _databaseNotification = client.GetDatabase($"{Environment.GetEnvironmentVariable("NOTIFICATION_MONGODB_DATABASE_NAME")}");
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<Company> Company
        {
            get
            {
                return _database.GetCollection<Company>("company");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<CommonData> CommonData
        {
            get
            {
                return _database.GetCollection<CommonData>("commondata");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<Lead> Lead
        {
            get
            {
                return _database.GetCollection<Lead>("lead");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<ActivityHistoryLead> ActivityHistoryLead
        {
            get
            {
                return _database.GetCollection<ActivityHistoryLead>("activity_history_lead");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<ContactLead> ContactLead
        {
            get
            {
                return _database.GetCollection<ContactLead>("contact_lead");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<ChatLead> ChatLead
        {
            get
            {
                return _database.GetCollection<ChatLead>("chat_lead");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<SMSLog> SMSLog
        {
            get
            {
                return _databaseNotification.GetCollection<SMSLog>("sms_log");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<EmailLog> EmailLog
        {
            get
            {
                return _databaseNotification.GetCollection<EmailLog>("email_log");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<ProductCategory> ProductCategory
        {
            get
            {
                return _database.GetCollection<ProductCategory>("product_category");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<Product> Product
        {
            get
            {
                return _database.GetCollection<Product>("product");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<Team> Team
        {
            get
            {
                return _database.GetCollection<Team>("team");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMongoCollection<TeamUsers> TeamUsers
        {
            get
            {
                return _database.GetCollection<TeamUsers>("team_users");
            }
        }
    }
}
