using Bridgeon.Dtos.UserProfile;
using Bridgeon.Models;
using Bridgeon.Utils;

namespace Bridgeon.Services.UserProfile
{
    public interface IUserProfileService
    {
        Task<ApiResponse<User>> GetUserProfileAsync(int userId);
        Task<ApiResponse<User>> UpdateUserProfileAsync(int userId, UserProfileUpdateDto updateDto);
        Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto);
        Task<ApiResponse<User>> UpdateProfileImageAsync(int userId, string imageUrl);


        Task<ApiResponse<IEnumerable<object>>> GetAllProfileImagesAsync();

    }


}