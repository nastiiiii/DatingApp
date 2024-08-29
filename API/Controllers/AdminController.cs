using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<AppUser> userManager) :BaseAPI
{
    [Authorize(Policy = "RequireAdminRole")]
    [HttpGet("users-with-roles")]
    public async Task<ActionResult> GetUsersWithRoles()
    {
        var users = await userManager.Users.OrderBy(x => x.UserName).Select(x => new
        {
            x.Id,
            Username = x.UserName,
            Roles = x.UserRoles.Select(x => x.Role.Name).ToList()
        }).ToListAsync();
        return Ok(users);
    }

    [Authorize(Policy = "RequireAdminRole")]
    [HttpPost("edit-roles/{username}")]
    public async Task<ActionResult> EditRoles(string username, string roles)
    {
        if (string.IsNullOrEmpty(roles)) return BadRequest("You must provide roles");
      
        var selectedRoles = roles.Split(",").ToArray();
        
        var user = await userManager.FindByNameAsync(username);
        if (user == null) return BadRequest("User not found");
        
        var userRoles = await userManager.GetRolesAsync(user);
        
        var result = await userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
        
        if(!result.Succeeded) return BadRequest("Failed to add roles");
       
        result = await userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
        
        return result.Succeeded ? Ok(await userManager.GetRolesAsync(user)) : BadRequest("Failed to remove roles");
    }
    
    
    [Authorize(Policy = "ModeratePhotoRole")]
    [HttpGet("photos-to-moderate")]
    public ActionResult GetPhotosForModeration()
    {
        return Ok("Moderators or admins can be accessed");
    }
}