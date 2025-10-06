using API.Controllers.InterfacesManagers;
using API.DAO;
using API.Errors;
using API.Helpers; // si tu as un dossier Helpers dans le namespace API.Services, ajuste l’using
using Api.Managers.InterfacesDao;
using Api.Managers.InterfacesHelpers;
using Api.Managers.InterfacesServices;
using API.Managers.InterfacesServices;
using API.Services;
using Microsoft.AspNetCore.HttpOverrides;

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
    spotifyClientId: cfg["Spotify:ClientId"] ?? throw new ArgumentNullException($"Spotify:ClientId configuration is missing."),
    spotifyRedirectUri: cfg["Spotify:RedirectUri"] ??
                        throw new ArgumentNullException($"Spotify:RedirectUri configuration is missing."),
    spotifyAuthorizeEndpoint: cfg.GetValue<string>("Spotify:AuthorizeEndpoint", "https://accounts.spotify.com/authorize") ??
                              throw new ArgumentNullException($"Spotify:AuthorizeEndpoint configuration is missing."),
    spotifyTokenEndpoint: cfg.GetValue<string>("Spotify:TokenEndpoint", "https://accounts.spotify.com/api/token") ??
                          throw new ArgumentNullException($"Spotify:TokenEndpoint configuration is missing."),
    deeplinkSchemeHost: cfg.GetValue<string>("Deeplink:SchemeHost", "swipez://oauth-callback/spotify") ??
                        throw new ArgumentNullException($"Deeplink:SchemeHost configuration is missing."),
    pkceTtlMinutes: cfg.GetValue("Security:PkceTtlMinutes", 10),
    sessionTtlMinutes: cfg.GetValue("Security:SessionTtlMinutes", 60)
);
builder.Services.AddSingleton(configService);

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
builder.Services.AddScoped<IAccessTokenDao, AccessTokenDao>();
builder.Services.AddScoped<IPlaylistSelectionDao, PlaylistSelectionDao>();
builder.Services.AddScoped<IPlaylistCacheDao, PlaylistCacheDao>();
builder.Services.AddScoped<IUserProfileCacheDao, UserProfileCacheDao>();
builder.Services.AddScoped<IDenylistedRefreshDao, DenylistedRefreshDao>();


// Services métier
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<ITokenDenyListService, TokenDenyListService>();
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<ITransactionRunner, MySqlTransactionRunner>();


// Helpers
builder.Services.AddScoped<ICryptoHelper, CryptoHelper>();
builder.Services.AddScoped<IUrlBuilderHelper, UrlBuilderHelper>();
builder.Services.AddScoped<IDeeplinkHelper, DeeplinkHelper>();
builder.Services.AddScoped<ISpotifyOAuthHelper, SpotifyOAuthHelper>();

// Error mapping
builder.Services.AddSingleton<IErrorMapper, DefaultErrorMapper>();

// Managers
builder.Services.AddScoped<IAuthManager, AuthManager>();

WebApplication app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions {
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Swagger en Dev
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Global error handling middleware
app.UseMiddleware<API.Middleware.ErrorHandlingMiddleware>();

app.UseRouting();
app.UseCors("Default");
app.MapControllers();

app.Run();