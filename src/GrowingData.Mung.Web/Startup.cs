using System.Collections.Generic;

using System;
using System.IO;
using Microsoft.AspNetCore.StaticFiles;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

using GrowingData.Mung.Web.Areas.Dashboards.Controllers;


namespace GrowingData.Mung.Web {
	public class Startup {

		public Startup(IHostingEnvironment env) {

			var appRootPath = new DirectoryInfo(env.WebRootPath).Parent.FullName;

			// Set up configuration sources.
			var builder = new ConfigurationBuilder()
				.SetBasePath(appRootPath)
				.AddEnvironmentVariables()
				.AddJsonFile("config.json");
			//All environment variables in the process's context flow in as configuration values.
			//

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

			Console.WriteLine("ConfigureServices");

			services.AddAuthentication();

			var mvc = services.AddMvc();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
			Console.WriteLine("Configure");
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment()) {
				//app.UseBrowserLink();
				app.UseStatusCodePages();
				app.UseDeveloperExceptionPage();
			} else {
				app.UseStatusCodePages();
				app.UseDeveloperExceptionPage();
			}



			//app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

			// Static file handlers and mime types
			var sfo = new StaticFileOptions {
				ContentTypeProvider = new FileExtensionContentTypeProvider()
			};
			((FileExtensionContentTypeProvider)sfo.ContentTypeProvider).Mappings.Add(
				new KeyValuePair<string, string>(".less", "text/css"));

			app.UseStaticFiles(sfo);
			// 

			app.UseCookieAuthentication(new CookieAuthenticationOptions() {
				AutomaticAuthenticate = true,
				LoginPath = $"/{Urls.LOGIN}",
				LogoutPath = $"/{Urls.LOGOUT}",
			});

			app.UseDeveloperExceptionPage();
			app.UseMvc(routes => {
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

		}

	}
}
