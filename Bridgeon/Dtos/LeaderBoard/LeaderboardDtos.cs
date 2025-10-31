// Dtos/Leaderboard/LeaderboardDtos.cs
using System;

namespace Bridgeon.Dtos.Leaderboard
{
    public class LeaderboardDto
    {
        public int Rank { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string ProfileImageUrl { get; set; }
        public decimal TotalScore { get; set; }
        public DateTime LatestReviewDate { get; set; }
    }

    public class DetailedLeaderboardDto : LeaderboardDto
    {
        public decimal AcademicScore { get; set; }
        public decimal ReviewScore { get; set; }
        public decimal TaskScore { get; set; }
        public int ReviewCount { get; set; }
    }
}