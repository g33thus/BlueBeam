// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.10.2

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using BlueBeam.Bots;
using BlueBeam.Dialogs;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Extensions.Configuration;

namespace BlueBeam
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment env)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //var dataStore = new CosmosDbStorage(cosmosOptions);

            var StorageConnectionString = Configuration.GetSection("StorageConnectionString").Get<string>();
            var StorageContainerName = Configuration.GetSection("StorageContainerName").Get<string>();
            var dataStore1 = new AzureBlobStorage(StorageConnectionString, StorageContainerName);

            var userState = new UserState(dataStore1);
            var conversationState = new ConversationState(dataStore1);

            services.AddSingleton(dataStore1);
            services.AddHttpClient();

            // Create the User state. (Used in this bot's Dialog implementation.)
            services.AddSingleton(new UserState(dataStore1));

            // Create the Conversation state. (Used by the Dialog system itself.)
            services.AddSingleton<ConversationState>( new ConversationState(dataStore1));

            // The MainDialog that will be run by the bot.
            services.AddSingleton<MainDialog>();

            services.AddTransient<CreateStudioProjectDialog>();
            services.AddTransient<AddUsertoProjectDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
