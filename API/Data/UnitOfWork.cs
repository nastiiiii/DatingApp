using API.Interfaces;

namespace API.Data;

public class UnitOfWork(DataContext context, IUserRepo userRepo, IMessageRepo messageRepo, ILikesRepo likesRepo) :IUnitOfWork
{
    public IUserRepo UserRepo => userRepo;
    public IMessageRepo MessageRepo => messageRepo;
    public ILikesRepo LikesRepo => likesRepo;
    
    public async Task<bool> CompleteAsync()
    {
        return await context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}