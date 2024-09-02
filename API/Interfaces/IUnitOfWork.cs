namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepo UserRepo { get; }
    IMessageRepo MessageRepo { get; }
    ILikesRepo LikesRepo { get; }
    Task<bool> CompleteAsync();
    bool HasChanges();
}