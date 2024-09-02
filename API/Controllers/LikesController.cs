using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class LikesController(IUnitOfWork unitOfWork) : BaseAPI
{
    [HttpPost("{targetUserId:int}")]
    public async Task<ActionResult> ToggleLike(int targetUserId)
    {
        var sourceUserId = User.GetUserId();
        if (sourceUserId == targetUserId) return BadRequest("You cannot like yourself");
        var existingLike = await unitOfWork.LikesRepo.GetUserLike(sourceUserId, targetUserId);
        if (existingLike == null)
        {
            var like = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUserId
            };
            unitOfWork.LikesRepo.AddLike(like); 
        }
        else
        {
            unitOfWork.LikesRepo.DeleteLike(existingLike);
        }

        if (await  unitOfWork.CompleteAsync()) return Ok();
        return BadRequest("Failed to update likes");
    }

    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<int>>> GetCurrentUserLikeId()
    {
        return Ok(await unitOfWork.LikesRepo.GetCurrentUserLikeIds(User.GetUserId()));
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
    {
        likesParams.UserId = User.GetUserId();
        var users = await unitOfWork.LikesRepo.GetUserLikes(likesParams);
        
        Response.AddPaginationHeader(users);
        
        return Ok(users);
    }
    
}