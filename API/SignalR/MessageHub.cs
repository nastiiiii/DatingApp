using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR;

public class MessageHub(IUnitOfWork unitOfWork, IMapper mapper, IHubContext<PresenceHub> presenceHub) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext?.Request.Query["User"];
        if (Context.User == null || string.IsNullOrEmpty(otherUser)) throw new Exception("Cannot join group");
       
        var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        var group = await AddToGroup(groupName);
        
        await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

        var messages = await unitOfWork.MessageRepo.GetMessageThread(Context.User.GetUsername(), otherUser!);
        
        if(unitOfWork.HasChanges()) await unitOfWork.CompleteAsync();
        
        await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var httpContext = Context.GetHttpContext();
        var username = Context.User.GetUsername() ?? throw new Exception("Cannot get user");
        if (username == createMessageDto.RecipientUsername) throw new HubException("You cannot send message to yourself");
        var sender = await unitOfWork.UserRepo.GetUserByUsername(username);
        var recipient = await unitOfWork.UserRepo.GetUserByUsername(createMessageDto.RecipientUsername);

        if (recipient == null || sender == null || sender.UserName == null || recipient.UserName == null)
            throw new HubException("Invalid recipient");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content,
        };
        
        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await unitOfWork.MessageRepo.GetMessageGroup(groupName);

        if (group != null && group.Connections.Any(x => x.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
            if (connections != null && connections?.Count != null)
            {
                await presenceHub.Clients.Clients(connections).SendAsync("NewMessageReceived", new {username = sender.UserName, knownAs = sender.KnownAs});
            }
        }
        
        unitOfWork.MessageRepo.AddMessage(message);
        if (await unitOfWork.CompleteAsync())
        {
            await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
        }
        throw new HubException("Unable to save message");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var group = await RemoveFromGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
        await base.OnDisconnectedAsync(exception);
    }

    private string GetGroupName(string caller, string? other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var username = Context.User?.GetUsername() ?? throw new Exception("Cannot get user");
        var group = await unitOfWork.MessageRepo.GetMessageGroup(groupName);
        var connection = new Connection {ConnectionId = Context.ConnectionId, Username = username};

        if (group == null)
        {
            group = new Group { Name = groupName };
            unitOfWork.MessageRepo.AddGroup(group);
        }
        
        group.Connections.Add(connection);
        if (await unitOfWork.CompleteAsync()) return group;
        throw new HubException("Unable to save message");
    }

    private async Task<Group> RemoveFromGroup()
    {
        var group = await unitOfWork.MessageRepo.GetGroupForConnection(Context.ConnectionId);
         var connection = group?.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
         if (connection != null && group != null) 
         {
             unitOfWork.MessageRepo.RemoveConnection(connection);
             if(await unitOfWork.CompleteAsync()) return group;
         }
        throw new Exception("Unable to remove from group");
    }
}