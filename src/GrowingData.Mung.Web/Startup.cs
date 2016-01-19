using System.Collections.Generic;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using GrowingData.Mung.Web.Areas.Dashboards.Controllers;


namespace GrowingData.Mung.Web {
	public class Startup {
		public Startup(IHostingEnvironment env) {

			// Set up configuration sources.
			var builder = new ConfigurationBuilder()
				.AddJsonFile("config.json")
				//All environment variables in the process's context flow in as configuration values.
				.AddEnvironmentVariables();

			Configuration = builder.Build();


			DatabaseContext.Initialize(
				Configuration["Data:mung:ConnectionString"],
				Configuration["Data:mung_events:ConnectionString"]
			);

			MungApp.Initialize(env);
		}

		public IConfigurationRoot Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {

			services.AddAuthentication();


			var mvc = services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment()) {
				//app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			} else {

				app.UseExceptionHandler("/Home/Error");
			}



			app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

			// Static file handlers and mime types
			var sfo = new StaticFileOptions {
				ContentTypeProvider = new FileExtensionContentTypeProvider()
			};
			((FileExtensionContentTypeProvider)sfo.ContentTypeProvider).Mappings.Add(
				new KeyValuePair<string, string>(".less", "text/css"));

			app.UseStaticFiles(sfo);
			// 

			app.UseCookieAuthentication(options => {
				options.AutomaticAuthenticate = true;
				options.LoginPath = $"/{Urls.LOGIN}";
				options.LogoutPath = $"/{Urls.LOGOUT}";
			});

			app.UseDeveloperExceptionPage();
			app.UseMvc(routes => {
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

		}

		// Entry point for the application.
		public static void Main(string[] args) {
			WebApplication.Run<Startup>(args);
		}
	}
}
