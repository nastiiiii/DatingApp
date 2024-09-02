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
public class MessagesController(IUnitOfWork unitOfWork, IMapper mapper) : BaseAPI
{
    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUsername();
        if (username == createMessageDto.RecipientUsername) return BadRequest("You cannot send message to yourself");
        var sender = await unitOfWork.UserRepo.GetUserByUsernameAsync(username);
        var recipient = await unitOfWork.UserRepo.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
            return BadRequest("Invalid recipient");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };
        
        unitOfWork.MessageRepo.AddMessage(message);
        if (await unitOfWork.CompleteAsync()) return Ok(mapper.Map<MessageDto>(message));
        return BadRequest("Unable to save message");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages([FromQuery] MessageParams messageParams)
    {
        messageParams.Username = User.GetUsername();

        var messages = await unitOfWork.MessageRepo.GetMessagesForUser(messageParams);
        Response.AddPaginationHeader(messages);
        return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
        var currentUsername = User.GetUsername();
        return Ok(await unitOfWork.MessageRepo.GetMessageThread(currentUsername, username));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
        var username = User.GetUsername();
        
        var message = await unitOfWork.MessageRepo.GetMessage(id);
        
        if (message == null) return BadRequest("Cannot delete message");
        if (message.SenderUsername != username && message.RecipientUsername != username) 
            return Forbid("Invalid sender or recipient");

        if (message.SenderUsername == username) message.SenderDeleted = true;
        if (message.RecipientUsername == username) message.RecipientDeleted = true;
        
        if (message is {SenderDeleted: true, RecipientDeleted: true}) unitOfWork.MessageRepo.DeleteMessage(message);

        if (await unitOfWork.CompleteAsync()) return Ok();
        
        return BadRequest("Unable to save message");
    }
}