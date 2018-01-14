using System;
using System.Reflection;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using OGame.Auth.Contexts;
using OGame.Auth.Models;
using OGame.Repositories.Contexts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OGame.Configuration.Contracts.Settings;
using OGame.Repositories;
using OGame.Repositories.Interfaces;
using OGame.Services;
using OGame.Services.Interfaces;

namespace OGame.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.Configure<ClientSettings>(configuration.GetSection("Client"));
            services.Configure<TokenSettings>(configuration.GetSection("Tokens"));
            services.Configure<AccountSettings>(configuration.GetSection("Account"));
            return services;
        }

        public static IServiceCollection ConfigureDbContexts(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<SecurityContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly("OGame.Auth")));

            services.AddDbContext<ScheduledTasksContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly("OGame.Repositories")));

            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.MigrationsAssembly("OGame.Repositories")));

            return services;
        }

        public static IServiceCollection ConfigureAuth(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, ApplicationUserRole>()
                .AddEntityFrameworkStores<SecurityContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequiredUniqueChars = 6;

                options.User.RequireUniqueEmail = true;
            });

            services.AddAuthentication(cfg =>
                {
                    cfg.DefaultScheme = IdentityConstants.ApplicationScheme;
                    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = configuration["Tokens:Issuer"],
                        ValidAudience = configuration["Tokens:Issuer"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"])),
                        ValidateLifetime = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            return services;
        }

        public static IServiceCollection ConfigureRepositories(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<IRunnerRepository, RunnerRepository>();
            return services;
        }

        public static IServiceCollection ConfigureServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<IIdProvider, IdProvider>();
            services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IRunnerService, RunnerService>();
            return services;
        }

        public static IServiceCollection ConfigureMapping(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAutoMapper(x => x.AddProfiles("OGame.Api", "OGame.Services"));
            return services;
        }
    }
}
