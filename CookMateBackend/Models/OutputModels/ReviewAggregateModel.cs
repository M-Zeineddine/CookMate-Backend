using CookMateBackend.Models.InputModels;

namespace CookMateBackend.Models.OutputModels
{
    public class ReviewAggregateModel
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<UserReviewModel>? UserReviews { get; set; }
    }
}
