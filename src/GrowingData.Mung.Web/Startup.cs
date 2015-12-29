using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GrowingData.Mung.Web.Models;
using GrowingData.Mung.Web.Services;

namespace GrowingData.Mung.Web {
	public class Startup {
		public Startup(IHostingEnvironment env) {
			Console.WriteLine("Yo, startup");

			// Set up configuration sources.
			var builder = new ConfigurationBuilder()
				.AddJsonFile("config.json")
				//All environment variables in the process's context flow in as configuration values.
				.AddEnvironmentVariables();

			Configuration = builder.Build();
			//_platform = new Platform(runtimeEnvironment);

			DatabaseContext.Initialize(Configuration["Data:DefaultConnection:ConnectionString"]);
		}

		public IConfigurationRoot Configuration { get; set; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {

			services.AddAuthentication();


			services.AddMvc();

			//// Add application services.
			//services.AddTransient<IEmailSender, AuthMessageSender>();
			//services.AddTransient<ISmsSender, AuthMessageSender>();
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

				//// For more details on creating database during deployment see http://go.microsoft.com/fwlink/?LinkID=615859
				//try {
				//    using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
				//        .CreateScope()) {
				//        serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
				//             .Database.Migrate();
				//    }
				//} catch { }
			}

			app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

			app.UseStaticFiles();
			app.UseCookieAuthentication(options => {
				options.AutomaticAuthenticate = true;
				options.LoginPath = "/signin";
				options.LogoutPath = "/signout";
			});

			//app.Run(context => {
			//	return Task.Run(() => {
			//		Console.WriteLine("app.Run");
			//		MungAuthContext.InitializeAuthentication(context);
			//	});
			//});

			//app.UseIdentity();
			// To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

			app.UseMvc(routes => {
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});

		}

		// Entry point for the application.
		public static void Main(string[] args) {
			Console.WriteLine("Yo, main");
			WebApplication.Run<Startup>(args);
		}
	}
}
