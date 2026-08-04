namespace Dhole.Reports.Api.Extensions;

internal static class HttpContextExtensions
{
    public static Guid? GetCurrentUserId(this HttpContext context)
    {
        var value = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst("user_id")?.Value
            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
