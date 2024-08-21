using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;
[Authorize]
public class UsersController(DataContext context, IUserRepo userRepo) : BaseAPI
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers()
    {
        return Ok(await userRepo.GetUsersAsync());
    }
    
    [HttpGet ("{id:int}")]
    public async Task<ActionResult<AppUser>> GetUser(int id)
    {
        var user = await userRepo.GetUserById(id);
        if (user == null) return NotFound();
        return user;
    }
    
    [HttpGet ("{username}")]
    public async Task<ActionResult<AppUser>> GetUser(string username)
    {
        var user = await userRepo.GetUserByUsername(username);
        if (user == null) return NotFound();
        return user;
    }
}