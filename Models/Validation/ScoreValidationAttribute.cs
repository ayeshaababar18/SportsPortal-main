using System.ComponentModel.DataAnnotations;

namespace SportsPortal.Models.Validation
{
    public class ScoreValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return ValidationResult.Success;
            }

            string score = value.ToString();

            // Check for Cricket format: Runs/Wickets
            if (score.Contains("/"))
            {
                var parts = score.Split('/');
                if (parts.Length != 2)
                {
                    return new ValidationResult("Invalid score format. Use 'Runs/Wickets' for Cricket (e.g., 120/5).");
                }

                if (int.TryParse(parts[1], out int wickets))
                {
                    if (wickets < 0 || wickets > 10)
                    {
                        return new ValidationResult("Cricket score must have wickets between 0 and 10.");
                    }
                }
                else
                {
                    return new ValidationResult("Wickets must be a valid number.");
                }
                
                if (int.TryParse(parts[0], out int runs))
                {
                    if (runs < 0)
                    {
                         return new ValidationResult("Runs cannot be negative.");
                    }
                }
                else
                {
                    return new ValidationResult("Runs must be a valid number.");
                }

            }
            // Check for simple integer scores (Football, etc.)
            else 
            {
                if (int.TryParse(score, out int points))
                {
                     if (points < 0)
                    {
                        return new ValidationResult("Score cannot be negative.");
                    }
                }
                // If it's not an integer and not containing '/', we assume it's valid text (maybe "Rain" or specialized format not yet defined), 
                // but usually scores are numbers. Let's strict it a bit? 
                // The user only asked specifically about wickets and "no int in name". 
                // I'll stick to the wicket logic primarily.
            }

            return ValidationResult.Success;
        }
    }
}
