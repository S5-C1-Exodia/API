using API.Managers.InterfacesServices;

namespace API.Middleware;

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IErrorMapper mapper,
    IClockService clock,
    IHostEnvironment env)
{
    private readonly RequestDelegate _next = next ?? throw new ArgumentNullException(nameof(next));
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IErrorMapper _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    private readonly IClockService _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    private readonly IHostEnvironment _env = env ?? throw new ArgumentNullException(nameof(env));

    public async Task Invoke(HttpContext context)
    {
        string correlationId = GetOrCreateCorrelationId(context);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            bool includeDetails = _env.IsDevelopment();
            DateTime now = _clock.GetUtcNow();

            int status;
            ApiError error = _mapper.Map(ex, correlationId, includeDetails, now, out status);

            // Log
            _logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            // Write response
            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.WriteIndented = false;

            string payload = JsonSerializer.Serialize(error, options);
            await context.Response.WriteAsync(payload);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        string header = context.Request.Headers["X-Correlation-Id"]!;
        if (string.IsNullOrWhiteSpace(header))
        {
            string generated = Guid.NewGuid().ToString("N");
            context.Response.Headers["X-Correlation-Id"] = generated;
            return generated;
        }

        context.Response.Headers["X-Correlation-Id"] = header;
        return header;
    }
}