using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface IUserRepo
{
    void Update (AppUser user);
    Task<bool> SaveChangesAsync ();
    Task<IEnumerable<AppUser>> GetUsersAsync ();
    Task<AppUser?> GetUserById (int id);
    Task<AppUser?> GetUserByUsername (string username);

    Task<IEnumerable<MemberDto>> GetMembersAsync();
    Task<MemberDto?> GetMemberAsync (string username);
}