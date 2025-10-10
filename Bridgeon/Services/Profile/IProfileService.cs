using Bridgeon.DTOs;
using System.Threading.Tasks;

namespace Bridgeon.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileDto> GetUserProfileAsync(int userId);
        Task<ProfileDto> CreateProfileAsync(ProfileDto profileDto);
        Task<ProfileDto> UpdateProfileAsync(int userId, ProfileDto profileDto);
        Task<bool> DeleteProfileAsync(int profileId, int currentUserId, string currentUserRole);
    }
}