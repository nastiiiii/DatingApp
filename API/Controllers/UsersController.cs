using System.Security.Claims;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService) : BaseAPI
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
    {
        userParams.CurrentUsername = User.GetUsername();
     
        var users = await unitOfWork.UserRepo.GetMembersAsync(userParams);
        
        Response.AddPaginationHeader(users);
        
        return Ok(users);
    }
    
    [HttpGet ("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await unitOfWork.UserRepo.GetMemberAsync(username);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await unitOfWork.UserRepo.GetUserByUsername(User.GetUsername());

        if (user == null) return BadRequest("Could not find user");

        mapper.Map(memberUpdateDto, user);

        if (await unitOfWork.CompleteAsync()) return NoContent();

        return BadRequest("Failed to update the user");
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await unitOfWork.UserRepo.GetUserByUsername(User.GetUsername());
        if (user == null) return BadRequest("Could not find user");
        
        var result = await photoService.AddPhotoAsync(file);
        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
        };
        if (user.Photos.Count == 0) photo.IsMain = true;
        user.Photos.Add(photo);
        if (await unitOfWork.CompleteAsync()) return CreatedAtAction(nameof(GetUser),new 
        { username = user.UserName }, mapper.Map<PhotoDto>(photo));
        
        return BadRequest("Failed to add photo");
    }

    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await unitOfWork.UserRepo.GetUserByUsername(User.GetUsername());
        if (user == null) return BadRequest("Could not find user");
        
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if(photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");
        
        var currentMainPhoto = user.Photos.FirstOrDefault(x => x.IsMain);
        if (currentMainPhoto != null) currentMainPhoto.IsMain = false;
        photo.IsMain = true;
        
        if (await unitOfWork.CompleteAsync()) return NoContent();
        return BadRequest("Failed to set main photo");
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await unitOfWork.UserRepo.GetUserByUsername(User.GetUsername());
        if (user == null) return BadRequest("Could not find user");
        
        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if(photo == null || photo.IsMain) return BadRequest("Cannot use this as main photo");

        if (photo.PublicId != null)
        {
            var result = await photoService.DeletePhotoAsync(photo.PublicId);
            if (result.Error != null) return BadRequest(result.Error.Message);
        }
        user.Photos.Remove(photo);
        if (await unitOfWork.CompleteAsync()) return Ok();
        return BadRequest("Failed to delete photo");
    }
    
}