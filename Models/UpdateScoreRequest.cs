using System.ComponentModel.DataAnnotations;
using SportsPortal.Models.Validation;

namespace SportsPortal.Models
{
    public class UpdateScoreRequest
    {
        public int MatchID { get; set; }
        [ScoreValidation]
        public string? ScoreA { get; set; }
        [ScoreValidation]
        public string? ScoreB { get; set; }
        
        [RegularExpression("^(Scheduled|Live|Finished)$", ErrorMessage = "Status must be 'Scheduled', 'Live', or 'Finished'.")]
        public string? Status { get; set; }
    }
}