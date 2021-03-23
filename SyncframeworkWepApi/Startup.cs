using BIT.Data.Services;
using BIT.Xpo.Providers.OfflineDataSync;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SyncframeworkWepApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        AutoCreateOption autoCreateOption = AutoCreateOption.SchemaAlreadyExists;
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            IDataStore dataStore = XpoDefault.GetConnectionProvider(Configuration.GetConnectionString("ConnectionString"), autoCreateOption);
            services.AddSingleton(new WebApiDataStoreService(dataStore));

            var DataStore = SyncDataStore.CreateProviderFromString(Configuration.GetConnectionString("Sync"), AutoCreateOption.DatabaseAndSchema, out _) as ISyncDataStore;
            services.AddSingleton<ISyncDataStore>(DataStore);
            services.AddSingleton<IObjectSerializationService>(new CompressXmlObjectSerializationService());
            
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SyncframeworkWepApi", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SyncframeworkWepApi v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
