using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace PollApp.Api.Extensions;

public static class ControllerBaseExtensions {
    public static string ExtractClaim<T> (this T controller, string type) where T : ControllerBase {
        var result = controller.HttpContext.User.Claims.FirstOrDefault(c => c.Type == type)!.Value;
        return result;
    }
}