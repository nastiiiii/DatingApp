using System.Security.Claims;

namespace API.Extensions;

public static class ClaimPrincipleExtensions
{
    public static string GetUsername(this ClaimsPrincipal principal)
    {
       var username = principal.FindFirstValue(ClaimTypes.Name) 
                      ?? throw new Exception("Cannot get username from token");
       return username;
    }
    
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) 
                       ?? throw new Exception("Cannot get id from token"));
        return userId;
    }
}