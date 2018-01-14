using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using OGame.Configuration;

namespace OGame.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureDbContexts(Configuration);
            services.ConfigureAuth(Configuration);
            services.ConfigureSettings(Configuration);

            //services.AddAutoMapper(typeof(Startup));
            services.ConfigureMapping(Configuration);
            

            services.ConfigureRepositories(Configuration);
            services.ConfigureServices(Configuration);

            //services.AddDbContext<SecurityContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
            //        sqlOptions => sqlOptions.MigrationsAssembly("OGame.Auth")));

            //services.AddIdentity<ApplicationUser, ApplicationUserRole>()
            //    .AddEntityFrameworkStores<SecurityContext>()
            //    .AddDefaultTokenProviders();

            //services.AddDbContext<ScheduledTasksContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
            //        sqlOptions => sqlOptions.MigrationsAssembly("OGame.Repositories")));


            //services.Configure<IdentityOptions>(options =>
            //{
            //    options.Password.RequireDigit = true;
            //    options.Password.RequiredLength = 8;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = true;
            //    options.Password.RequireLowercase = true;
            //    options.Password.RequiredUniqueChars = 6;

            //    options.User.RequireUniqueEmail = true;
            //});

            //services.AddAuthentication(cfg =>
            //    {
            //        cfg.DefaultScheme = IdentityConstants.ApplicationScheme;
            //        cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //        cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    })
            //    .AddJwtBearer(cfg =>
            //    {
            //        cfg.RequireHttpsMetadata = false;
            //        cfg.SaveToken = true;
            //        cfg.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidIssuer = Configuration["Tokens:Issuer"],
            //            ValidAudience = Configuration["Tokens:Issuer"],
            //            IssuerSigningKey =
            //                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"])),
            //            ValidateLifetime = true,
            //            ValidateIssuer = true,
            //            ValidateAudience = true,
            //            ClockSkew = TimeSpan.Zero
            //        };
            //    });

            //services.AddAutoMapper(typeof(Startup));

            

            //services.AddScoped<IEmailRepository, EmailRepository>();

            //services.AddSingleton<IIdProvider, IdProvider>();
            //services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
            //services.AddTransient<IEmailService, EmailService>();

            services.AddCors();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(conf =>
                conf.WithOrigins("http://localhost:8080")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
            );

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}