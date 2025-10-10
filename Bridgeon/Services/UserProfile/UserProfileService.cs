using Bridgeon.Dtos.UserProfile;
using Bridgeon.Models;
using Bridgeon.Repositories;
using Bridgeon.Services.UserProfile;
using Bridgeon.Utils;
using Microsoft.AspNetCore.Identity;

namespace Bridgeon.Services
{
    namespace Bridgeon.Services.UserProfile
    {
        public class UserProfileService : IUserProfileService
        {
            private readonly IUserProfileRepository _userProfileRepository;
            private readonly IPasswordHasher<User> _passwordHasher;

            public UserProfileService(IUserProfileRepository userProfileRepository, IPasswordHasher<User> passwordHasher)
            {
                _userProfileRepository = userProfileRepository;
                _passwordHasher = passwordHasher;
            }


            public async Task<ApiResponse<User>> GetUserProfileAsync(int userId)
            {
                try
                {
                    var user = await _userProfileRepository.GetUserByIdAsync(userId);
                    if (user == null)
                        return new ApiResponse<User> { Status = ApiStatusCodes.NotFound, Message = "User not found" };

                    user.PasswordHash = null;

                    return new ApiResponse<User>
                    {
                        Status = ApiStatusCodes.Success,
                        Message = "User profile retrieved successfully",
                        Data = user
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<User>
                    {
                        Status = ApiStatusCodes.ServerError,
                        Message = $"Error retrieving user profile: {ex.Message}"
                    };
                }
            }

            public async Task<ApiResponse<User>> UpdateUserProfileAsync(int userId, UserProfileUpdateDto updateDto)
            {
                try
                {
                    var user = await _userProfileRepository.GetUserByIdAsync(userId);
                    if (user == null)
                        return new ApiResponse<User> { Status = ApiStatusCodes.NotFound, Message = "User not found" };

                    // Check if email is already taken by another user
                    var existingUser = await _userProfileRepository.GetUserByEmailAsync(updateDto.Email);
                    if (existingUser != null && existingUser.Id != userId)
                        return new ApiResponse<User> { Status = ApiStatusCodes.BadRequest, Message = "Email already taken" };

                    user.FullName = updateDto.FullName;
                    user.Email = updateDto.Email;

                    var updated = await _userProfileRepository.UpdateUserAsync(user);
                    if (!updated)
                        return new ApiResponse<User> { Status = ApiStatusCodes.ServerError, Message = "Failed to update user" };

                    user.PasswordHash = null;

                    return new ApiResponse<User>
                    {
                        Status = ApiStatusCodes.Success,
                        Message = "User profile updated successfully",
                        Data = user
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<User>
                    {
                        Status = ApiStatusCodes.ServerError,
                        Message = $"Error updating user profile: {ex.Message}"
                    };
                }
            }

            public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
            {
                try
                {
                    var user = await _userProfileRepository.GetUserByIdAsync(userId);
                    if (user == null)
                        return new ApiResponse<bool> { Status = ApiStatusCodes.NotFound, Message = "User not found" };

                    // Verify current password with BCrypt
                    if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
                        return new ApiResponse<bool> { Status = ApiStatusCodes.BadRequest, Message = "Current password incorrect" };

                    // Hash new password
                    var newHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
                    var updated = await _userProfileRepository.UpdatePasswordAsync(userId, newHash);

                    if (!updated)
                        return new ApiResponse<bool> { Status = ApiStatusCodes.ServerError, Message = "Failed to update password" };

                    return new ApiResponse<bool>
                    {
                        Status = ApiStatusCodes.Success,
                        Message = "Password changed successfully",
                        Data = true
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<bool>
                    {
                        Status = ApiStatusCodes.ServerError,
                        Message = $"Error changing password: {ex.Message}"
                    };
                }
            }

            public async Task<ApiResponse<User>> UpdateProfileImageAsync(int userId, string imageUrl)
            {
                try
                {
                    var user = await _userProfileRepository.GetUserByIdAsync(userId);
                    if (user == null)
                        return new ApiResponse<User> { Status = ApiStatusCodes.NotFound, Message = "User not found" };

                    user.ProfileImageUrl = imageUrl;
                    var updated = await _userProfileRepository.UpdateUserAsync(user);

                    if (!updated)
                        return new ApiResponse<User> { Status = ApiStatusCodes.ServerError, Message = "Failed to update profile image" };

                    user.PasswordHash = null;

                    return new ApiResponse<User>
                    {
                        Status = ApiStatusCodes.Success,
                        Message = "Profile image updated successfully",
                        Data = user
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<User>
                    {
                        Status = ApiStatusCodes.ServerError,
                        Message = $"Error updating profile image: {ex.Message}"
                    };
                }
            }


            public async Task<ApiResponse<IEnumerable<object>>> GetAllProfileImagesAsync()
            {
                try
                {
                    var users = await _userProfileRepository.GetAllUsersAsync();
                    var result = users.Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        u.Role,
                        u.IsBlocked,
                        u.ProfileImageUrl
                    });

                    return new ApiResponse<IEnumerable<object>>
                    {
                        Status = ApiStatusCodes.Success,
                        Message = "All user profile images fetched successfully",
                        Data = result
                    };
                }
                catch (Exception ex)
                {
                    return new ApiResponse<IEnumerable<object>>
                    {
                        Status = ApiStatusCodes.ServerError,
                        Message = $"Error fetching profile images: {ex.Message}"
                    };
                }
            }

        }
    }
}