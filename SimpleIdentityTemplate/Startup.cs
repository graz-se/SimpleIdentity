using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Graz.SimpleIdentity.Template
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{

			AuthSettings authSettings = new AuthSettings()
			{
				AppDomain = "http://localhost:51444",
				Authority = "https://aawvsfzja.accounts.ondemand.com",
				ClientId = "19800736-1099-403b-afb3-12909e8afdd4",
				ClientSecret = "u1Fuofiqdh8J.Z:oq-:wZonm@yxiPQ",
				Expiry = new DateTime(2021,12,31),
				AuthsPerDay = 175,
				License = ""
			};

			services.AddHttpContextAccessor();

			services.AddSingleton<AuthSettings>(authSettings);
			
			SAPAuth auth = new SAPAuth(authSettings);
			services.AddScoped<SAPAuth>(auth.GetImplementationFactory());

			services.AddAuthentication(options =>
			{
				options.DefaultScheme = "Cookies";
				options.DefaultChallengeScheme = "oidc";
			})
			.AddCookie("Cookies")
			.AddOpenIdConnect("oidc", auth.Options);

			services.AddRazorPages();
			services.AddServerSideBlazor();

			services.AddMvcCore(options =>
			{
				var policy = new AuthorizationPolicyBuilder()
					 .RequireAuthenticatedUser()
					 .Build();
				options.Filters.Add(new AuthorizeFilter(policy));
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
			}

			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapBlazorHub();
				endpoints.MapFallbackToPage("/_Host");
			});
		}
	}
}
