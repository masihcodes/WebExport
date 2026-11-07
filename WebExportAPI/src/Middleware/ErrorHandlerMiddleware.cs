using System.Net.Sockets;
using Models.DTOs;
using Npgsql;

namespace WebExportAPI.Middleware;

public class ErrorHandlerMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ILogger<ErrorHandlerMiddleware> logger)
    {
        try
        {
            await next(context);
        }
        catch (BadHttpRequestException ex)
            when (ex.StatusCode == StatusCodes.Status400BadRequest && !context.Response.HasStarted)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                ErrorCode = "INVALID_PARAMETER",
                Message = "Die Anfrage enthält ungültige oder fehlende Angaben."
            });
        }
        catch (Exception ex)
            when (IsDatabaseSocketFailure(ex) && !context.Response.HasStarted && !context.RequestAborted.IsCancellationRequested)
        {
            logger.LogError(ex, "Die Datenbank ist nicht erreichbar.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                ErrorCode = "DATABASE_UNAVAILABLE",
                Message = "Die Datenbank ist nicht erreichbar."
            });
        }
        catch (Exception ex)
            when (ex is not BadHttpRequestException && !context.Response.HasStarted && !context.RequestAborted.IsCancellationRequested)
        {
            logger.LogError(ex, "Ein unerwarteter Fehler ist aufgetreten.");

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ErrorResponseDto
            {
                ErrorCode = "INTERNAL_ERROR",
                Message = "Ein interner Fehler ist aufgetreten."
            });
        }
    }

    private bool IsDatabaseSocketFailure(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is NpgsqlException &&
                current is not PostgresException &&
                current.GetBaseException() is SocketException)
            {
                return true;
            }
        }

        return false;
    }
}

