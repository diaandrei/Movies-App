using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Movies.Api.Mapping;
using Movies.Api.Swagger;
using Movies.Application.Database;
using Movies.Application.Models;
using Movies.Application.Services;
using Movies.Identity;
using Serilog.Sinks.MSSqlServer;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.ObjectModel;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var config = builder.Configuration;
        var keyVaultUrl = config["KeyVault:VaultUrl"];
        string tokenSecret = null!, issuer = null!, audience = null!, connectionString = null!, omdbApiKey = null!;

        SecretClient secretClient = null!;

        if (!string.IsNullOrEmpty(keyVaultUrl))
        {
            secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

            tokenSecret = secretClient.GetSecret("Movies-JWTKey").Value.Value;
            issuer = secretClient.GetSecret("Movies-JwtIssuer").Value.Value;
            audience = secretClient.GetSecret("Movies-JwtAudience").Value.Value;
            omdbApiKey = secretClient.GetSecret("Omdb-ApiKey").Value.Value;
            connectionString = secretClient.GetSecret("Movies-DBConnectionString")?.Value?.Value!;
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = config["ConnectionStrings:Database"];
        }

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException("Database connection string is missing or empty.");
        }

        builder.Services.AddSingleton(secretClient);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true },
                columnOptions: new ColumnOptions
                {
                    AdditionalColumns = new Collection<SqlColumn>
                    {
                        new SqlColumn { ColumnName = "UserName", DataType = System.Data.SqlDbType.NVarChar, DataLength = 50, AllowNull = true }
                    }
                }
            )
            .CreateLogger();

        builder.Host.UseSerilog();

        builder.Services.AddSingleton<JwtConfigurationService>();

        builder.Services.AddTransient<TokenGenerator>();

        builder.Services.AddDbContext<MoviesDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<MoviesDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtConfig = builder.Services.BuildServiceProvider().GetRequiredService<JwtConfigurationService>();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.TokenSecret)),
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtConfig.Issuer,
                ValidAudience = jwtConfig.Audience,
                ValidateIssuer = true,
                ValidateAudience = true
            };
        });

        builder.Services.AddAuthorization();

        builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwagger>();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Movies API", Version = "v1" });
            c.CustomSchemaIds(type => type.FullName);
            c.SchemaFilter<ExcludeSystemTypesSchemaFilter>();
            c.DocInclusionPredicate((docName, apiDesc) => !apiDesc.RelativePath!.Contains("admin"));
        });

        var configMap = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile(new MappingProfile());
            cfg.SourceMemberNamingConvention = LowerUnderscoreNamingConvention.Instance;
            cfg.DestinationMemberNamingConvention = PascalCaseNamingConvention.Instance;
            cfg.AllowNullCollections = true;
        });
        var mapper = configMap.CreateMapper();
        builder.Services.AddSingleton(mapper);

        builder.Services.AddCors(option =>
        {
            option.AddPolicy("AllowOrigin", origin => origin.AllowAnyOrigin()
                                                            .AllowAnyMethod()
                                                            .AllowAnyHeader());
        });

        builder.Services.AddHttpClient<OmdbService>(client =>
        {
            client.BaseAddress = new Uri("https://api.omdbapi.com/");
        });

        builder.Services.AddScoped<IOmdbService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<OmdbService>>();
            var client = provider.GetRequiredService<HttpClient>();
            return new OmdbService(logger, client, omdbApiKey);
        });

        builder.Services.AddControllers();

        builder.Services.AddApplication(connectionString, omdbApiKey);

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors("AllowOrigin");
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseMiddleware<ValidationMappingMiddleware>();
        app.UseMiddleware<TokenMiddleware>();
        app.MapControllers();

        app.Run();
    }
}
