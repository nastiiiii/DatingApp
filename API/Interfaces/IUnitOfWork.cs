namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepo UserRepo { get; }
    IMessageRepo MessageRepo { get; }
    ILikesRepo LikesRepo { get; }
    IPhotoRepository PhotoRepository {get;}
    Task<bool> CompleteAsync();
    bool HasChanges();
}