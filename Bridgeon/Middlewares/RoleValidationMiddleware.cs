using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Bridgeon.Services;
using Microsoft.AspNetCore.Authorization;
using System;

namespace Bridgeon.Middleware
{
    public class RoleValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserService userService)
        {
            var endpoint = context.GetEndpoint();

            // ✅ Skip for anonymous endpoints
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                await _next(context);
                return;
            }

            // 1️⃣ Check if user authenticated
            if (!context.User.Identity.IsAuthenticated)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: User not authenticated.");
                return;
            }

            // 2️⃣ Extract userId from token
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Invalid user ID.");
                return;
            }

            // 3️⃣ Fetch user from DB (latest role)
            var user = userService.GetById(userId);
            if (user == null)
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: User not found.");
                return;
            }

            if (user.IsBlocked)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: User is blocked.");
                return;
            }

            // 4️⃣ Get current request path
            var path = context.Request.Path;

            // 🧠 Role revalidation (important)
            // Check real DB role, not JWT role
            var actualRole = user.Role?.Trim()?.ToLower();

            // ✅ Restrict admin routes
            if (path.StartsWithSegments("/api/admin") &&
                actualRole != "admin")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Only Admin can access.");
                return;
            }

            // ✅ Allow both Admin & Mentor for mentor endpoints
            if (path.StartsWithSegments("/api/mentor"))
            {
                if (actualRole != "mentor" && actualRole != "admin")
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Forbidden: Only Mentor or Admin can access.");
                    return;
                }
            }

            // ✅ Restrict user endpoints
            if (path.StartsWithSegments("/api/user") &&
                actualRole != "user")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Only User can access.");
                return;
            }

            // ✅ Restrict role change endpoint
            if (path.StartsWithSegments("/api/roles/change") &&
                context.Request.Method == "POST" &&
                actualRole != "admin")
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Only Admin can change roles.");
                return;
            }

            // ✅ Continue request
            await _next(context);
        }
    }
}


/////////////////////////////////////////////