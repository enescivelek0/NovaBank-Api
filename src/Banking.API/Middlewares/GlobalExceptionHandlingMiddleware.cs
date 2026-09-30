using System.Net;
using System.Text.Json;
using Banking.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Banking.API.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Status = (int)statusCode,
            Title = "Sunucu Hatası",
            Detail = "İşlem sırasında beklenmeyen bir hata oluştu."
        };

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails.Status = (int)statusCode;
                problemDetails.Title = "Doğrulama Hatası (Validation Error)";
                problemDetails.Detail = "Bir veya daha fazla alan doğrulama kurallarından geçemedi.";

                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );

                problemDetails.Extensions["errors"] = errors;
                _logger.LogWarning(exception, "Doğrulama hatası gerçekleşti: {Path}", context.Request.Path);
                break;

            case InsufficientBalanceException insufficientBalanceException:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails.Status = (int)statusCode;
                problemDetails.Title = "Yetersiz Bakiye";
                problemDetails.Detail = insufficientBalanceException.Message;
                problemDetails.Extensions["currentBalance"] = insufficientBalanceException.CurrentBalance;
                problemDetails.Extensions["requestedAmount"] = insufficientBalanceException.RequestedAmount;
                _logger.LogWarning("Yetersiz bakiye hatası: {Detail}", insufficientBalanceException.Message);
                break;

            case InvalidAccountOperationException invalidAccountOpEx:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails.Status = (int)statusCode;
                problemDetails.Title = "Geçersiz Hesap İşlemi";
                problemDetails.Detail = invalidAccountOpEx.Message;
                _logger.LogWarning("Geçersiz hesap işlemi: {Detail}", invalidAccountOpEx.Message);
                break;

            case InvalidIbanException invalidIbanEx:
                statusCode = HttpStatusCode.BadRequest;
                problemDetails.Status = (int)statusCode;
                problemDetails.Title = "Geçersiz IBAN";
                problemDetails.Detail = invalidIbanEx.Message;
                _logger.LogWarning("Geçersiz IBAN: {Detail}", invalidIbanEx.Message);
                break;

            case EntityNotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                problemDetails.Status = (int)statusCode;
                problemDetails.Title = "Kayıt Bulunamadı (Not Found)";
                problemDetails.Detail = notFoundEx.Message;
                _logger.LogWarning("Kayıt bulunamadı: {Detail}", notFoundEx.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                problemDetails.Status = (int)statusCode;
                problemDetails.Title = "Yetkisiz Erişim (Unauthorized)";
                problemDetails.Detail = "Bu işlem için geçerli yetkiniz bulunmamaktadır.";
                _logger.LogWarning("Yetkisiz erişim teşebbüsü.");
                break;

            default:
                _logger.LogError(exception, "İşlenemeyen istisna meydana geldi: {Message}", exception.Message);
                problemDetails.Detail = exception.Message;
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var responseJson = JsonSerializer.Serialize(problemDetails, jsonOptions);
        await context.Response.WriteAsync(responseJson);
    }
}
