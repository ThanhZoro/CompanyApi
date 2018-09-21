using System;
using System.IO;
using System.Linq;
using System.Net;
using ApiESReadService.Models;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Consumers;
using Contracts.Commands;
using Contracts.Models;
using MassTransit;
using MassTransit.Util;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using CompanyApi.Data;
using CompanyApi.Models;
using CompanyApi.Repository;
using CompanyApi.Services;
using Serilog;
using Serilog.Context;
using Serilog.Exceptions;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.SystemConsole.Themes;
using Swashbuckle.AspNetCore.Swagger;

namespace CompanyApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console(theme: ConsoleTheme.None)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri($"http://{Environment.GetEnvironmentVariable("ES_HOST")}:{Environment.GetEnvironmentVariable("ES_PORT")}/"))
                {
                    AutoRegisterTemplate = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv6,
                    IndexFormat = "logstash-api-company-{0:yyyy}"
                })
            .CreateLogger();
        }

        /// <summary>
        /// 
        /// </summary>
        public IContainer ApplicationContainer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = $"{Environment.GetEnvironmentVariable("IS_SERVER")}";
                    options.RequireHttpsMetadata = false;
                    options.ApiName = "api";
                    options.ApiSecret = "secret";
                });

            services.AddDataProtection()
                .SetApplicationName("api-company")
                .PersistKeysToFileSystem(new DirectoryInfo(@"/var/dpkeys/"));

            services.AddCors(options =>
            {
                // this defines a CORS policy called "default"
                options.AddPolicy("default", policy =>
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod().WithExposedHeaders("Authorization");
                });
            });

            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = $"{Environment.GetEnvironmentVariable("REDIS_HOST")}:{Environment.GetEnvironmentVariable("REDIS_PORT")}";
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "CompanyApi", Version = "v1" });
                var filePath = Path.Combine(AppContext.BaseDirectory, "CompanyApi.xml");
                c.IncludeXmlComments(filePath);
            });

            var builder = new ContainerBuilder();
            builder.RegisterType<CompanyRepository>().As<ICompanyRepository>();
            builder.RegisterType<CommonDataRepository>().As<ICommonDataRepository>();
            builder.RegisterType<LeadRepository>().As<ILeadRepository>();
            builder.RegisterType<ContactLeadRepository>().As<IContactLeadRepository>();
            builder.RegisterType<ChatLeadRepository>().As<IChatLeadRepository>();
            builder.RegisterType<ProductCategoryRepository>().As<IProductCategoryRepository>();
            builder.RegisterType<ProductRepository>().As<IProductRepository>();
            builder.RegisterType<TeamRepository>().As<ITeamRepository>();
            builder.RegisterType<TeamUsersRepository>().As<ITeamUsersRepository>();
            builder.RegisterType<ActivityHistoryLeadRepository>().As<IActivityHistoryLeadRepository>();

            builder.RegisterType<ApplicationDbContext>().WithParameter("connectionString", $"mongodb://{Environment.GetEnvironmentVariable("MONGODB_USERNAME")}:{Environment.GetEnvironmentVariable("MONGODB_PASSWORD")}@{Environment.GetEnvironmentVariable("COMPANY_MONGODB_HOST")}:{Environment.GetEnvironmentVariable("COMPANY_MONGODB_PORT")}")
                   .WithParameter("database", $"{Environment.GetEnvironmentVariable("COMPANY_MONGODB_DATABASE_NAME")}");

            services.Configure<ElasticSearchSettings>(options =>
            {
                options.Host = Environment.GetEnvironmentVariable("ES_HOST");
                options.Port = Environment.GetEnvironmentVariable("ES_PORT");
            });

            builder.RegisterType<EmailSender>().As<IEmailSender>()
                    .WithParameter("sendGridUser", "apikey")
                    .WithParameter("sendGridKey", "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");
            builder.RegisterType<SMSSender>().As<ISMSSender>();

            var timeout = TimeSpan.FromSeconds(30);

            //mass transit endpoint
            //company
            services.AddScoped<CreateCompanyConsumer>();

            builder.RegisterType<UploadLogoCompanyConsumer>();
            builder.Register(c => new MessageRequestClient<IUploadLogoCompany, Company>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/upload_logo_company"), timeout))
                .As<IRequestClient<IUploadLogoCompany, Company>>()
                .SingleInstance();

            services.AddScoped<UpdateCompanyConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateCompany, Company>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_company"), timeout))
                .As<IRequestClient<IUpdateCompany, Company>>()
                .SingleInstance();

            services.AddScoped<UpdateSettingsCompanyConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateSettingsCompany, Company>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_setting_company"), timeout))
                .As<IRequestClient<IUpdateSettingsCompany, Company>>()
                .SingleInstance();

            services.AddScoped<UpdateMailSettingsConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateMailSettings, Company>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_mail_setting_company"), timeout))
                .As<IRequestClient<IUpdateMailSettings, Company>>()
                .SingleInstance();

            //common data
            services.AddScoped<CreateCommonDataConsumer>();
            builder.Register(c => new MessageRequestClient<ICreateCommonData, CommonData>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/create_common_data"), timeout))
                .As<IRequestClient<ICreateCommonData, CommonData>>()
                .SingleInstance();

            services.AddScoped<UpdateCommonDataConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateCommonData, CommonData>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_common_data"), timeout))
                .As<IRequestClient<IUpdateCommonData, CommonData>>()
                .SingleInstance();

            services.AddScoped<DeleteCommonDataConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteCommonData, CommonData>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_common_data"), timeout))
                .As<IRequestClient<IDeleteCommonData, CommonData>>()
                .SingleInstance();

            services.AddScoped<CreateDefaultCommonDataConsumer>();

            //lead
            builder.RegisterType<CreateLeadConsumer>();
            builder.Register(c => new MessageRequestClient<ICreateLead, LeadEdited>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/create_lead"), timeout))
                .As<IRequestClient<ICreateLead, LeadEdited>>()
                .SingleInstance();

            builder.RegisterType<UpdateLeadConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateLead, LeadEdited>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_lead"), timeout))
                .As<IRequestClient<IUpdateLead, LeadEdited>>()
                .SingleInstance();

            builder.RegisterType<CreateActivityHistoryLeadConsumer>();

            builder.RegisterType<DeleteLeadConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteLead, ILeadDeleted>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_lead"), timeout))
                .As<IRequestClient<IDeleteLead, ILeadDeleted>>()
                .SingleInstance();

            builder.RegisterType<ImportLeadConsumer>();
            builder.Register(c => new MessageRequestClient<IImportLead, ILeadImported>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/import_lead"), timeout))
                .As<IRequestClient<IImportLead, ILeadImported>>()
                .SingleInstance();

            builder.RegisterType<AddSupportStaffConsumer>();
            builder.Register(c => new MessageRequestClient<IAddSupportStaff, Lead>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/add_support_staff"), timeout))
             .As<IRequestClient<IAddSupportStaff, Lead>>()
             .SingleInstance();

            builder.RegisterType<RemoveSupportStaffConsumer>();
            builder.Register(c => new MessageRequestClient<IRemoveSupportStaff, Lead>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/remove_support_staff"), timeout))
              .As<IRequestClient<IRemoveSupportStaff, Lead>>()
              .SingleInstance();

            builder.RegisterType<AddStaffInChargeConsumer>();
            builder.Register(c => new MessageRequestClient<IAddStaffInCharge, IStaffInChargeAdded>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/add_staffInCharse"), timeout))
              .As<IRequestClient<IAddStaffInCharge, IStaffInChargeAdded>>()
              .SingleInstance();

            //contact lead
            builder.RegisterType<CreateContactLeadConsumer>();
            builder.Register(c => new MessageRequestClient<ICreateContactLead, ContactLead>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/create_contact_lead"), timeout))
             .As<IRequestClient<ICreateContactLead, ContactLead>>()
             .SingleInstance();

            builder.RegisterType<UpdateContactLeadConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateContactLead, ContactLead>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_contact_lead"), timeout))
             .As<IRequestClient<IUpdateContactLead, ContactLead>>()
             .SingleInstance();

            builder.RegisterType<DeleteContactLeadConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteContactLead, ContactLead>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_contact_lead"), timeout))
            .As<IRequestClient<IDeleteContactLead, ContactLead>>()
            .SingleInstance();

            builder.RegisterType<UploadAvatarContactLeadConsumer>();
            builder.Register(c => new MessageRequestClient<IUploadAvatarContactLead, ContactLead>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/upload_avatar_contact_lead"), timeout))
            .As<IRequestClient<IUploadAvatarContactLead, ContactLead>>()
            .SingleInstance();

            //chat lead
            builder.RegisterType<CreateChatLeadConsumer>();
            
            //send notification
            builder.RegisterType<SendMailConsumer>();
            builder.RegisterType<SendSMSConsumer>();
            services.AddScoped<CountSendActiveAccountConsumer>();

            //account
            builder.Register(c => new MessageRequestClient<IGetUsers, IUsersGetted>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/get_users"), timeout))
           .As<IRequestClient<IGetUsers, IUsersGetted>>()
           .SingleInstance();

            //product category
            builder.RegisterType<CreateProductCategoryConsumer>();
            builder.Register(c => new MessageRequestClient<ICreateProductCategory, ProductCategory>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/create_product_category"), timeout))
            .As<IRequestClient<ICreateProductCategory, ProductCategory>>()
            .SingleInstance();

            builder.RegisterType<UpdateProductCategoryConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateProductCategory, ProductCategory>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_product_category"), timeout))
            .As<IRequestClient<IUpdateProductCategory, ProductCategory>>()
            .SingleInstance();

            builder.RegisterType<DeleteProductCategoryConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteProductCategory, IProductCategoryDeleted>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_product_category"), timeout))
            .As<IRequestClient<IDeleteProductCategory, IProductCategoryDeleted>>()
            .SingleInstance();

            //product
            builder.RegisterType<CreateProductConsumer>();
            builder.Register(c => new MessageRequestClient<ICreateProduct, Product>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/create_product"), timeout))
            .As<IRequestClient<ICreateProduct, Product>>()
            .SingleInstance();

            builder.RegisterType<UpdateProductConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateProduct, Product>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_product"), timeout))
            .As<IRequestClient<IUpdateProduct, Product>>()
            .SingleInstance();

            builder.RegisterType<DeleteProductConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteProduct, IProductDeleted>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_product"), timeout))
            .As<IRequestClient<IDeleteProduct, IProductDeleted>>()
            .SingleInstance();
            
            //team
            builder.RegisterType<CreateTeamConsumer>();
            builder.Register(c => new MessageRequestClient<ICreateTeam, Team>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/create_team"), timeout))
            .As<IRequestClient<ICreateTeam, Team>>()
            .SingleInstance();

            builder.RegisterType<UpdateTeamConsumer>();
            builder.Register(c => new MessageRequestClient<IUpdateTeam, Team>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/update_team"), timeout))
            .As<IRequestClient<IUpdateTeam, Team>>()
            .SingleInstance();

            builder.RegisterType<DeleteTeamConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteTeam, ITeamDeleted>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_team"), timeout))
            .As<IRequestClient<IDeleteTeam, ITeamDeleted>>()
            .SingleInstance();

            //team users
            builder.RegisterType<EditTeamUsersConsumer>();
            builder.Register(c => new MessageRequestClient<IEditTeamUsers, ITeamUsersEdited>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/edit_team_users"), timeout))
            .As<IRequestClient<IEditTeamUsers, ITeamUsersEdited>>()
            .SingleInstance();

            builder.RegisterType<DeleteTeamUsersConsumer>();
            builder.Register(c => new MessageRequestClient<IDeleteTeamUsers, ITeamUsersDeleted>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/delete_team_users"), timeout))
            .As<IRequestClient<IDeleteTeamUsers, ITeamUsersDeleted>>()
            .SingleInstance();

            builder.Register(c => new MessageRequestClient<ICheckAccessRight, CheckAccessRightResponse>(c.Resolve<IBus>(), new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/check_access_right"), timeout))
                .As<IRequestClient<ICheckAccessRight, CheckAccessRightResponse>>()
                .SingleInstance();

            builder.Register(context =>
            {
                return Bus.Factory.CreateUsingRabbitMq(sbc =>
                {
                    var host = sbc.Host(new Uri($"rabbitmq://{Environment.GetEnvironmentVariable("RABBITMQ_HOST")}/"), h =>
                    {
                        h.Username(Environment.GetEnvironmentVariable("RABBITMQ_USERNAME"));
                        h.Password(Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD"));
                    });

                    //company
                    sbc.ReceiveEndpoint(host, "create_company", ep =>
                    {
                        ep.Consumer<CreateCompanyConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "upload_logo_company", ep =>
                    {
                        ep.Consumer<UploadLogoCompanyConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_company", ep =>
                    {
                        ep.Consumer<UpdateCompanyConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_setting_company", ep =>
                    {
                        ep.Consumer<UpdateSettingsCompanyConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_mail_setting_company", ep =>
                    {
                        ep.Consumer<UpdateMailSettingsConsumer>(context);
                    });

                    //common data
                    sbc.ReceiveEndpoint(host, "create_common_data", ep =>
                    {
                        ep.Consumer<CreateCommonDataConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_common_data", ep =>
                    {
                        ep.Consumer<UpdateCommonDataConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "delete_common_data", ep =>
                    {
                        ep.Consumer<DeleteCommonDataConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "create_defeaul_common_data", ep =>
                    {
                        ep.Consumer<CreateDefaultCommonDataConsumer>(context);
                    });

                    //lead
                    sbc.ReceiveEndpoint(host, "create_lead", ep =>
                    {
                        ep.Consumer<CreateLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_lead", ep =>
                    {
                        ep.Consumer<UpdateLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "lead_activity_history", ep =>
                     {
                         ep.Consumer<CreateActivityHistoryLeadConsumer>(context);
                     });
                    sbc.ReceiveEndpoint(host, "delete_lead", ep =>
                    {
                        ep.Consumer<DeleteLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "import_lead", ep =>
                    {
                        ep.Consumer<ImportLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "add_support_staff", ep =>
                    {
                        ep.Consumer<AddSupportStaffConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "remove_support_staff", ep =>
                    {
                        ep.Consumer<RemoveSupportStaffConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "add_staffInCharse", ep =>
                    {
                        ep.Consumer<AddStaffInChargeConsumer>(context);
                    });

                    //contact lead
                    sbc.ReceiveEndpoint(host, "create_contact_lead", ep =>
                    {
                        ep.Consumer<CreateContactLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_contact_lead", ep =>
                    {
                        ep.Consumer<UpdateContactLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "delete_contact_lead", ep =>
                    {
                        ep.Consumer<DeleteContactLeadConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "upload_avatar_contact_lead", ep =>
                    {
                        ep.Consumer<UploadAvatarContactLeadConsumer>(context);
                    });

                    //chat lead
                    sbc.ReceiveEndpoint(host, "lead_chat", ep =>
                     {
                         ep.Consumer<CreateChatLeadConsumer>(context);
                     });
                    
                    //send notification
                    sbc.ReceiveEndpoint(host, "send_mail", ep =>
                    {
                        ep.Consumer<SendMailConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "send_sms", ep =>
                    {
                        ep.Consumer<SendSMSConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "count_send_active_account", ep =>
                    {
                        ep.Consumer<CountSendActiveAccountConsumer>(context);
                    });

                    //product category
                    sbc.ReceiveEndpoint(host, "create_product_category", ep =>
                    {
                        ep.Consumer<CreateProductCategoryConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_product_category", ep =>
                    {
                        ep.Consumer<UpdateProductCategoryConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "delete_product_category", ep =>
                    {
                        ep.Consumer<DeleteProductCategoryConsumer>(context);
                    });

                    //product
                    sbc.ReceiveEndpoint(host, "create_product", ep =>
                    {
                        ep.Consumer<CreateProductConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_product", ep =>
                    {
                        ep.Consumer<UpdateProductConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "delete_product", ep =>
                    {
                        ep.Consumer<DeleteProductConsumer>(context);
                    });
                    
                    //team
                    sbc.ReceiveEndpoint(host, "create_team", ep =>
                    {
                        ep.Consumer<CreateTeamConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "update_team", ep =>
                    {
                        ep.Consumer<UpdateTeamConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "delete_team", ep =>
                    {
                        ep.Consumer<DeleteTeamConsumer>(context);
                    });

                    //team users
                    sbc.ReceiveEndpoint(host, "edit_team_users", ep =>
                    {
                        ep.Consumer<EditTeamUsersConsumer>(context);
                    });
                    sbc.ReceiveEndpoint(host, "delete_team_users", ep =>
                    {
                        ep.Consumer<DeleteTeamUsersConsumer>(context);
                    });
                });
            })
            .As<IBus>()
            .As<IBusControl>()
            .As<IPublishEndpoint>()
            .SingleInstance();
            //end mass transit endpoint


            builder.Populate(services);
            ApplicationContainer = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(ApplicationContainer);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="appLifetime"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CompanyApi V1");
                });
            }
            loggerFactory.AddSerilog();

            IPHostEntry local = Dns.GetHostEntry(Environment.GetEnvironmentVariable("LOADBALANCER"));
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All,
                RequireHeaderSymmetry = false,
                ForwardLimit = null,
                KnownProxies = { local.AddressList[0] }
            });
            app.Use(async (ctx, next) =>
            {
                using (LogContext.PushProperty("IPAddress", ctx.Connection.RemoteIpAddress))
                using (LogContext.PushProperty("UserName", ctx.User?.Claims?.FirstOrDefault(_ => _.Type == "userName")?.Value))
                {
                    await next();
                }
            });

            app.UseCors("default");
            app.UseAuthentication();
            app.UseMvc();
            //resolve the bus from the container
            var bus = ApplicationContainer.Resolve<IBusControl>();
            //start the bus
            var busHandle = TaskUtil.Await(() => bus.StartAsync());

            appLifetime.ApplicationStopped.Register(() => { busHandle.Stop(); ApplicationContainer.Dispose(); });
        }
    }
}
