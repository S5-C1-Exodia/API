using API.Managers.InterfacesServices;

namespace API.Middleware;

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Middleware for centralized error handling in HTTP requests.
/// Captures unhandled exceptions, logs them, maps them to API errors, and returns a standardized JSON response.
/// </summary>
/// <param name="next">The next middleware delegate in the pipeline.</param>
/// <param name="logger">Logger instance for error logging.</param>
/// <param name="mapper">Service to map exceptions to API error objects.</param>
/// <param name="clock">Service to provide the current UTC time.</param>
/// <param name="env">Host environment information (used to determine if details should be included).</param>
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

    /// <summary>
    /// Handles the HTTP request, catching and processing any unhandled exceptions.
    /// Logs the error, maps it to an API error, and writes a JSON response with appropriate status code and correlation ID.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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

            ApiError error = _mapper.Map(ex, correlationId, includeDetails, now, out int status);

            // Log
            _logger.LogError(ex, "Unhandled exception. CorrelationId={CorrelationId}", correlationId);

            // Write response
            context.Response.Clear();
            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            context.Response.Headers["X-Correlation-Id"] = correlationId;

            JsonSerializerOptions options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            string payload = JsonSerializer.Serialize(error, options);
            await context.Response.WriteAsync(payload);
        }
    }

    /// <summary>
    /// Retrieves the correlation ID from the request headers, or generates a new one if not present.
    /// Sets the correlation ID in the response headers.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    /// <returns>The correlation ID as a string.</returns>
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