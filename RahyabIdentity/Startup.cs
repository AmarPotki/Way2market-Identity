using System;
using System.Linq;
using System.Reflection;
using AspNetCoreRateLimit;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RahyabIdentity.Configuration;
using RahyabIdentity.Infrastructure;
using RahyabIdentity.Models;
using RahyabIdentity.Services;
namespace RahyabIdentity{
    public class Startup{
        public Startup(IConfiguration configuration){
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services){
            services.AddControllersWithViews();
            // configures IIS out-of-proc settings (see https://github.com/aspnet/AspNetCore/issues/14882)
            services.Configure<IISOptions>(iis => {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            // configures IIS in-proc settings
            services.Configure<IISServerOptions>(iis => {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });
            var connectionString = Configuration["ConnectionString"];
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            services.AddEntityFrameworkSqlServer()
                .AddDbContext<ApplicationDbContext>(options => {
                        options.UseSqlServer(connectionString,
                            sqlServerOptionsAction: sqlOptions => {
                                sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                                    errorNumbersToAdd: null);
                            });
                    },
                    ServiceLifetime
                        .Scoped //Showing explicitly that the DbContext is shared across the HTTP request scope (graph of objects started in the HTTP request));
                );
            services.AddIdentity<ApplicationUser, IdentityRole>(t=>
                {
                    t.SignIn.RequireConfirmedPhoneNumber = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            var builder = services.AddIdentityServer(options => {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.UserInteraction = new UserInteractionOptions
                    {
                        LogoutUrl = "/Account/Logout",
                        LoginUrl = "/Account/Login",
                        LoginReturnUrlParameter = "returnUrl"
                    };
                }).AddAspNetIdentity<ApplicationUser
                >()
                // .AddSigningCredential(new X509Certificate2(@".\Configuration\blueirvin.com.pfx", "Blu3Irvin.com"))
                // this adds the config data from DB (clients, resources)
                .AddConfigurationStore(options => {
                    options.ConfigureDbContext = db =>
                        db.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // this adds the operational data from DB (codes, tokens, consents)
                .AddOperationalStore(options => {
                    options.ConfigureDbContext = db =>
                        db.UseSqlServer(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // this enables automatic token cleanup. this is optional.
                    options.EnableTokenCleanup = true;
                    // options.TokenCleanupInterval = 15; // interval in seconds. 15 seconds useful for debugging
                });
            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            //services.AddAuthentication()
            //    .AddCookie("RahyabIdentity")
            //    .AddYahoo(options => {
            //       options.SignInScheme = "RahyabIdentity"; 
            //      //  options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        options.Scope.Add("openid");
            //        options.ClientId =
            //            "dj0yJmk9czJERU1mWU96VklwJmQ9WVdrOVFYTjNNbXRsTjJjbWNHbzlNQS0tJnM9Y29uc3VtZXJzZWNyZXQmc3Y9MCZ4PWM4"; //Configuration.GetValue<string>("ExternalProviders:YahooClientId");
            //        options.ClientSecret =
            //            "93b8987a03bdd24de0fd52b7939e20a3bb8e7661"; //Configuration.GetValue<string>("ExternalProviders:YahooSecretKey");
            //        // options.CallbackPath = Configuration.GetValue<string>("ExternalProviders:YahooCallbackPath");
            //    })
            //    .AddGoogle("Google", options => {
            //        options.SignInScheme = "RahyabIdentity";
            //       // options.SignInScheme =  IdentityServerConstants.ExternalCookieAuthenticationScheme;
            //        // options.ClientId = "758817582785-ulf7898ms3eq9g1sok8r0m33opqc4vqh.apps.googleusercontent.com"; //Configuration["Secret:GoogleClientId"];
            //        options.ClientId =
            //            "659563659230-7ro8eugsltgecicvob8216tgg7t3n9qa.apps.googleusercontent.com"; //Configuration["Secret:GoogleClientId"];
            //        // options.ClientSecret = "ALBdL_em3UAEb-ScsNN0BkKd"; //Configuration["Secret:GoogleClientSecret"];
            //        options.ClientSecret = "zmJmKuK8pooDOU4ePR6zyR8b"; //Configuration["Secret:GoogleClientSecret"];
            //    }).AddInstagram("Instagram", options => {
            //        options.SignInScheme = "RahyabIdentity";
            //        options.ClientId = "2605274459716910";
            //        options.ClientSecret = "52733ddece62aa24c84504e36fd61ffb";

            //        options.Scope.Remove("basic");
            //        options.Scope.Add("user_profile");
            //        options.Scope.Add("user_media");
            //    }); 

            // preserve OIDC state in cache (solves problems with AAD and URL lenghts)
            //services.AddOidcStateDataFormatterCache("aad");

            // add CORS policy for non-IdentityServer endpoints
            services.AddCors(options => {
                options.AddPolicy("api", policy => { policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            });
            services.AddTransient<IdentityErrorDescriber, FaIdentityErrorDescriber>();
            services.AddTransient<ISmsService, SmsService>();
            services.AddTransient<IRedirectUriValidator, Way2MarketRedirectValidator>();
            services.AddTransient<IProfileService, ProfileService>();


            //asp.netcoreRateLimite
            services.AddOptions();
            services.AddMemoryCache();
            //load ip rules from appsettings.json
           // services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            //load ip rules from appsettings.json
            //services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
        
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env){
            app.UseIpRateLimiting();

            if (env.IsDevelopment()){ app.UseDeveloperExceptionPage(); }
            else{
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("api");
            app.UseRouting();
            InitializeDatabase(app);
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                //endpoints.MapControllerRoute(
                //    name: "default",
                //    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapDefaultControllerRoute();
            });
        }
        private void InitializeDatabase(IApplicationBuilder app){
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope()){
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any()){
                    foreach (var client in ServiceConfiguration.GetClients(Configuration["SiteUrl"])){
                        context.Clients.Add(client.ToEntity());
                    }

                    context.SaveChanges();
                }

                if (!context.IdentityResources.Any()){
                    foreach (var resource in ServiceConfiguration.IdentityResources){
                        context.IdentityResources.Add(resource.ToEntity());
                    }

                    context.SaveChanges();
                }

                //if (!context.ApiResources.Any())
                //{
                //    foreach (var resource in ServiceConfiguration.GetApiResources())
                //    {
                //        context.ApiResources.Add(resource.ToEntity());
                //    }

                //    context.SaveChanges();
                //}
                var applicationDbContext = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                applicationDbContext.Database.Migrate();
                //if (!applicationDbContext.Users.Any())
                //{
                //    applicationDbContext.Users.AddRange(ServiceConfiguration.GetUsers());

                //    await applicationDbContext.SaveChangesAsync();
                //}
            }
        }
    }
}