using System;
using API.Controllers;
using API.Controllers.InterfacesManagers;
using API.DAO;
using API.Helpers; // si tu as un dossier Helpers dans le namespace API.Services, ajuste l’using
using API.Managers;
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// -----------------------------
// Program.cs (.NET 8, top-level)
// -----------------------------

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS (au besoin pour tests; adapte la policy)
builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            "Default",
            policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        );
    }
);

// HttpClient pour Spotify OAuth
builder.Services.AddHttpClient("spotify-oauth", client => { client.Timeout = TimeSpan.FromSeconds(15); });

// Configuration forte (singleton)
IConfiguration cfg = builder.Configuration;
IConfigService configService = new ConfigService(
    spotifyClientId: cfg["Spotify:ClientId"],
    spotifyRedirectUri: cfg["Spotify:RedirectUri"],
    spotifyAuthorizeEndpoint: cfg.GetValue<string>("Spotify:AuthorizeEndpoint", "https://accounts.spotify.com/authorize"),
    spotifyTokenEndpoint: cfg.GetValue<string>("Spotify:TokenEndpoint", "https://accounts.spotify.com/api/token"),
    deeplinkSchemeHost: cfg.GetValue<string>("Deeplink:SchemeHost", "swipez://oauth-callback/spotify"),
    pkceTtlMinutes: cfg.GetValue<int>("Security:PkceTtlMinutes", 10),
    sessionTtlMinutes: cfg.GetValue<int>("Security:SessionTtlMinutes", 60)
);
builder.Services.AddSingleton<IConfigService>(configService);

// Horloge / Audit / Ids
builder.Services.AddSingleton<IClockService, ClockService>();
builder.Services.AddSingleton<IAuditService, AuditService>();
builder.Services.AddSingleton<IIdGenerator, IdGenerator>();

// MySQL connection factory
string connectionString = cfg.GetConnectionString("Default");
builder.Services.AddSingleton<ISqlConnectionFactory>(sp =>
    new SqlConnectionFactory(connectionString)
);

// DAO
builder.Services.AddScoped<IPkceDao, PkceDao>();
builder.Services.AddScoped<ITokenDao, TokenDao>();
builder.Services.AddScoped<ISessionDao, SessionDao>();

// Services métier
builder.Services.AddScoped<ISessionService, SessionService>();

// Helpers
builder.Services.AddScoped<ICryptoHelper, CryptoHelper>();
builder.Services.AddScoped<IUrlBuilderHelper, UrlBuilderHelper>();
builder.Services.AddScoped<IDeeplinkHelper, DeeplinkHelper>();
builder.Services.AddScoped<ISpotifyOAuthHelper, SpotifyOAuthHelper>();

// Managers
builder.Services.AddScoped<IAuthManager, AuthManager>();

WebApplication app = builder.Build();

// Swagger en Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("Default");
app.MapControllers();

app.Run();