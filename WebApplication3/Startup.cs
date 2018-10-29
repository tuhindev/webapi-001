using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using WebApplication3.Data;
namespace WebApplication3
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<ITitemQuery, TitemQuery>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            var audienceConfig = Configuration.GetSection("Audience");

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"]));
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateIssuer = true,
                ValidIssuer = audienceConfig["Iss"],
                ValidateAudience = true,
                ValidAudience = audienceConfig["Aud"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
            };
            //services.AddAuthentication(o =>
            //{
            //    o.DefaultAuthenticateScheme = "TestKey";
            //})
            //services
            //.AddJwtBearer("TestKey", x =>
            //{
            //    x.RequireHttpsMetadata = false;
            //    x.TokenValidationParameters = tokenValidationParameters;
            //});

            //services.AddJwtBearerAuthentication(options =>
            //{
            //    options.TokenValidationParameters = tokenValidationParameters;
            //});
            //services.AddJwtBearerAuthentication()

            //services.AddAuthorization(options =>
            //{
            //    options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().Build();
            //});

            services.AddAuthentication(auth => {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
    .AddJwtBearer(jwt =>
    {
        jwt.RequireHttpsMetadata = false;
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = signingKey,
            ValidAudience = audienceConfig["Aud"],
            ValidIssuer = audienceConfig["Iss"],

            // When receiving a token, check that it is still valid.
            ValidateLifetime = true,

            // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time
            // when validating the lifetime. As we're creating the tokens locally and validating them on the same
            // machines which should have synchronised time, this can be set to zero. Where external tokens are
            // used, some leeway here could be useful.
            ClockSkew = TimeSpan.FromMinutes(0)
        };
    });

            services.AddAuthorization(auth =>
            {
                auth.AddPolicy("Bearer", new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                        .RequireAuthenticatedUser()
                        .Build());
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            //  app.UseJwtBearerAuthentication()
            app.UseAuthentication();
            app.UseAPIResponseWrapperMiddleware();
            app.UseMvc();
        }
    }
}
