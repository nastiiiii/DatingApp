using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IUserRepo
{
    void Update (AppUser user);
    Task<IEnumerable<AppUser>> GetUsersAsync ();
    Task<AppUser?> GetUserById (int id);
    Task<AppUser?> GetUserByUsername (string username);

    Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams);
    Task<MemberDto?> GetMemberAsync (string username);
}