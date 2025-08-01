using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Anima.Infrastructure.Auth;
using System.Text.Json;

namespace Anima.Infrastructure.Auth;

/// <summary>
/// Middleware для аутентификации через API ключи
/// </summary>
public class APIKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<APIKeyMiddleware> _logger;
    private readonly APIKeyService _apiKeyService;

    public APIKeyMiddleware(RequestDelegate next, ILogger<APIKeyMiddleware> logger, APIKeyService apiKeyService)
    {
        _next = next;
        _logger = logger;
        _apiKeyService = apiKeyService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Пропускаем аутентификацию для публичных эндпоинтов
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var authResult = await AuthenticateRequest(context);
        
        if (!authResult.IsValid)
        {
            await HandleUnauthorized(context, authResult.ErrorMessage);
            return;
        }

        // Добавляем информацию о пользователе в контекст
        context.Items["CurrentUser"] = new CurrentUser
        {
            UserId = authResult.UserId,
            Role = authResult.Role,
            UserName = authResult.UserName,
            Permissions = authResult.Permissions,
            IsAuthenticated = true
        };

        // Логируем аутентифицированный запрос
        _logger.LogInformation("Authenticated request: {Method} {Path} by {User} ({Role})", 
            context.Request.Method, context.Request.Path, authResult.UserId, authResult.Role);

        await _next(context);
    }

    private async Task<AuthResult> AuthenticateRequest(HttpContext context)
    {
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader))
        {
            return new AuthResult
            {
                IsValid = false,
                ErrorMessage = "Отсутствует заголовок Authorization"
            };
        }

        if (!authHeader.StartsWith("Bearer "))
        {
            return new AuthResult
            {
                IsValid = false,
                ErrorMessage = "Неверный формат токена. Используйте 'Bearer <token>'"
            };
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        
        // Определяем тип токена и валидируем
        // Если токен содержит дефисы или подчеркивания, это API ключ
        if (token.Contains('-') || token.Contains('_'))
        {
            return await _apiKeyService.ValidateAPIKeyAsync(token);
        }
        else if (token.Length == 32) // Сессия (32 символа без дефисов/подчеркиваний)
        {
            return await _apiKeyService.ValidateSessionAsync(token);
        }
        else // API ключ
        {
            return await _apiKeyService.ValidateAPIKeyAsync(token);
        }
    }

    private bool IsPublicEndpoint(PathString path)
    {
        var publicPaths = new[]
        {
            "/",
            "/health",
            "/api/health",
            "/swagger",
            "/api/docs",
            "/favicon.ico"
        };

        return publicPaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
    }

    private async Task HandleUnauthorized(HttpContext context, string errorMessage)
    {
        context.Response.StatusCode = 401;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Unauthorized",
            message = errorMessage,
            timestamp = DateTime.UtcNow,
            path = context.Request.Path.Value
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        _logger.LogWarning("Unauthorized request: {Method} {Path} - {Error}", 
            context.Request.Method, context.Request.Path, errorMessage);

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension методы для регистрации middleware
/// </summary>
public static class APIKeyMiddlewareExtensions
{
    public static IApplicationBuilder UseAPIKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<APIKeyMiddleware>();
    }
}

/// <summary>
/// Атрибут для проверки ролей
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRoleAttribute : Attribute
{
    public string[] Roles { get; }

    public RequireRoleAttribute(params string[] roles)
    {
        Roles = roles ?? throw new ArgumentNullException(nameof(roles));
    }
}

/// <summary>
/// Атрибут для проверки разрешений
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute
{
    public string[] Permissions { get; }

    public RequirePermissionAttribute(params string[] permissions)
    {
        Permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
    }
}

/// <summary>
/// Фильтр для проверки авторизации в контроллерах
/// </summary>
public class AuthorizationFilter : Microsoft.AspNetCore.Mvc.Filters.IActionFilter
{
    public void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
    {
        var currentUser = context.HttpContext.Items["CurrentUser"] as CurrentUser;
        
        if (currentUser == null || !currentUser.IsAuthenticated)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedObjectResult(new
            {
                error = "Authentication required",
                message = "Необходима аутентификация для доступа к этому ресурсу"
            });
            return;
        }

        // Проверяем требования к роли
        var roleAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<RequireRoleAttribute>()
            .FirstOrDefault();

        if (roleAttribute != null && !roleAttribute.Roles.Contains(currentUser.Role))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
            {
                error = "Insufficient privileges",
                message = $"Требуется роль: {string.Join(" или ", roleAttribute.Roles)}",
                currentRole = currentUser.Role
            }) { StatusCode = 403 };
            return;
        }

        // Проверяем требования к разрешениям
        var permissionAttribute = context.ActionDescriptor.EndpointMetadata
            .OfType<RequirePermissionAttribute>()
            .FirstOrDefault();

        if (permissionAttribute != null)
        {
            var hasPermission = permissionAttribute.Permissions.Any(p => currentUser.HasPermission(p));
            if (!hasPermission)
            {
                context.Result = new Microsoft.AspNetCore.Mvc.ObjectResult(new
                {
                    error = "Insufficient permissions",
                    message = $"Требуется разрешение: {string.Join(" или ", permissionAttribute.Permissions)}",
                    currentPermissions = currentUser.Permissions
                }) { StatusCode = 403 };
                return;
            }
        }
    }

    public void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
    {
        // Пост-обработка не требуется
    }
}

/// <summary>
/// Helper методы для работы с пользователем в контроллерах
/// </summary>
public static class ControllerExtensions
{
    public static CurrentUser? GetCurrentUser(this Microsoft.AspNetCore.Mvc.ControllerBase controller)
    {
        return controller.HttpContext.Items["CurrentUser"] as CurrentUser;
    }

    public static bool IsCreator(this Microsoft.AspNetCore.Mvc.ControllerBase controller)
    {
        var user = controller.GetCurrentUser();
        return user?.IsCreator == true;
    }

    public static bool HasPermission(this Microsoft.AspNetCore.Mvc.ControllerBase controller, string permission)
    {
        var user = controller.GetCurrentUser();
        return user?.HasPermission(permission) == true;
    }

    public static string GetUserId(this Microsoft.AspNetCore.Mvc.ControllerBase controller)
    {
        var user = controller.GetCurrentUser();
        return user?.UserId ?? "anonymous";
    }

    public static string GetUserRole(this Microsoft.AspNetCore.Mvc.ControllerBase controller)
    {
        var user = controller.GetCurrentUser();
        return user?.Role ?? "Anonymous";
    }
}