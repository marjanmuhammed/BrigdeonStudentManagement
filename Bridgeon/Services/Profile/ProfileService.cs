using System;
using System.Threading.Tasks;
using Bridgeon.Models;
using Bridgeon.DTOs;
using Bridgeon.Repositories.Interfaces;
using Bridgeon.Services.Interfaces;

namespace Bridgeon.Services.Implementations
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }

        public async Task<ProfileDto> GetUserProfileAsync(int userId)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new ArgumentException("Profile not found");

            return MapToDto(profile);
        }

        public async Task<ProfileDto> CreateProfileAsync(ProfileDto dto)
        {
            if (!await _profileRepository.UserExistsAsync(dto.UserId))
                throw new ArgumentException("User does not exist");

            var existingProfile = await _profileRepository.GetByUserIdAsync(dto.UserId);
            if (existingProfile != null)
                throw new InvalidOperationException("Profile already exists for this user");

            var profile = MapToEntity(dto);
            profile.CreatedAt = DateTime.UtcNow;
            profile.UpdatedAt = DateTime.UtcNow;

            var createdProfile = await _profileRepository.CreateAsync(profile);
            return MapToDto(createdProfile);
        }

        public async Task<ProfileDto> UpdateProfileAsync(int userId, ProfileDto dto)
        {
            var profile = await _profileRepository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new ArgumentException("Profile not found");

            profile.Email = dto.Email ?? profile.Email;
            profile.Phone = dto.Phone ?? profile.Phone;
            profile.Address = dto.Address ?? profile.Address;
            profile.Branch = dto.Branch ?? profile.Branch;
            profile.Space = dto.Space ?? profile.Space;
            profile.Week = dto.Week != 0 ? dto.Week : profile.Week;
            profile.Advisor = dto.Advisor ?? profile.Advisor;
            profile.Mentor = dto.Mentor ?? profile.Mentor;
            profile.Qualification = dto.Qualification ?? profile.Qualification;
            profile.Institution = dto.Institution ?? profile.Institution;
            profile.PassOutYear = dto.PassOutYear != 0 ? dto.PassOutYear : profile.PassOutYear;
            profile.GuardianName = dto.GuardianName ?? profile.GuardianName;
            profile.GuardianRelationship = dto.GuardianRelationship ?? profile.GuardianRelationship;
            profile.GuardianPhone = dto.GuardianPhone ?? profile.GuardianPhone;
            profile.UpdatedAt = DateTime.UtcNow;

            var updatedProfile = await _profileRepository.UpdateAsync(profile);
            return MapToDto(updatedProfile);
        }

        public async Task<bool> DeleteProfileAsync(int profileId, int currentUserId, string currentUserRole)
        {
            if (currentUserRole != "Admin")
                throw new UnauthorizedAccessException("Only admins can delete profiles");

            return await _profileRepository.DeleteAsync(profileId);
        }

        private ProfileDto MapToDto(Profile profile)
        {
            return new ProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Email = profile.Email,
                Phone = profile.Phone,
                Address = profile.Address,
                Branch = profile.Branch,
                Space = profile.Space,
                Week = profile.Week,
                Advisor = profile.Advisor,
                Mentor = profile.Mentor,
                Qualification = profile.Qualification,
                Institution = profile.Institution,
                PassOutYear = profile.PassOutYear,
                GuardianName = profile.GuardianName,
                GuardianRelationship = profile.GuardianRelationship,
                GuardianPhone = profile.GuardianPhone,
                CreatedAt = profile.CreatedAt,
                UpdatedAt = profile.UpdatedAt
            };
        }

        private Profile MapToEntity(ProfileDto dto)
        {
            return new Profile
            {
                UserId = dto.UserId,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                Branch = dto.Branch,
                Space = dto.Space,
                Week = dto.Week,
                Advisor = dto.Advisor,
                Mentor = dto.Mentor,
                Qualification = dto.Qualification,
                Institution = dto.Institution,
                PassOutYear = dto.PassOutYear,
                GuardianName = dto.GuardianName,
                GuardianRelationship = dto.GuardianRelationship,
                GuardianPhone = dto.GuardianPhone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}