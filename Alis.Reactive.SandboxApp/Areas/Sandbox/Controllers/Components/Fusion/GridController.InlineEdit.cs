using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Care-plan reviews: the Birch wing nurse moves a resident's next care-plan review, and the care team's
    /// calendar books the new day or refuses it. Every page starts from the reviews on file; a booking answers
    /// with the review as it now stands and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        // The Sunday that ends the week the page lists as due.
        private static readonly DateOnly CarePlanReviewWeekEnds = new(2026, 10, 18);

        [HttpGet("InlineEdit"), ActionName("InlineEdit")]
        public IActionResult CarePlanReviewsPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/InlineEdit.cshtml",
                new CarePlanReviewsViewModel
                {
                    DueThisWeek = CarePlanReviewsOnFile()
                        .Where(review => CarePlanReviewDate(review.NextReview) <= CarePlanReviewWeekEnds)
                        .Select(review => new CarePlanReviewDue(
                            $"review-due-{review.ResidentId}",
                            review.ResidentId,
                            $"{review.ResidentName}, {CarePlanReviewDate(review.NextReview).ToString("dddd", CultureInfo.InvariantCulture)}"))
                        .ToList()
                });

        [HttpGet("InlineEditReviews")]
        public IActionResult InlineEditReviews() => Ok(CarePlanReviewsOnFile());

        // The care team meets on weekdays: a review moved to a weekend, or left without a day, keeps the day on file.
        [HttpPost("InlineEditBooking")]
        public IActionResult InlineEditBooking([FromBody] CarePlanReviewBooking booking)
        {
            var moved = booking.Review;
            var onFile = CarePlanReviewsOnFile().FirstOrDefault(review => review.ResidentId == moved.ResidentId);
            if (onFile == null)
                return NotFound();

            var kept = $"{onFile.ResidentName}'s review stays on {CarePlanReviewDay(onFile.NextReview)}.";
            if (!DateOnly.TryParseExact(moved.NextReview, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day))
                return BadRequest(new CarePlanReviewAnswer { Review = onFile, Summary = $"Pick a day for the review. {kept}" });

            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return BadRequest(new CarePlanReviewAnswer { Review = onFile, Summary = $"The care team does not meet on {day.DayOfWeek}s. {kept}" });

            return Ok(new CarePlanReviewAnswer
            {
                Review = moved,
                Summary = $"{moved.ResidentName}'s care-plan review is booked for {CarePlanReviewDay(moved.NextReview)}."
            });
        }

        private static DateOnly CarePlanReviewDate(string day) => DateOnly.ParseExact(day, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        // "Friday, October 16", as the nurse reads a day.
        private static string CarePlanReviewDay(string day) =>
            CarePlanReviewDate(day).ToString("dddd, MMMM d", CultureInfo.InvariantCulture);

        private static List<CarePlanReview> CarePlanReviewsOnFile() => new()
        {
            new CarePlanReview { ResidentId = 6301, ResidentName = "Amina Patel", Suite = "201", CareLevel = "Assisted Living", NextReview = "2026-10-15" },
            new CarePlanReview { ResidentId = 6302, ResidentName = "Henry Liu", Suite = "204", CareLevel = "Memory Care", NextReview = "2026-10-16" },
            new CarePlanReview { ResidentId = 6303, ResidentName = "Grace Bennett", Suite = "207", CareLevel = "Assisted Living", NextReview = "2026-11-12" },
            new CarePlanReview { ResidentId = 6304, ResidentName = "Walter Grant", Suite = "210", CareLevel = "Skilled Nursing", NextReview = "2026-12-03" },
            new CarePlanReview { ResidentId = 6305, ResidentName = "Rosa Diaz", Suite = "212", CareLevel = "Independent", NextReview = "2027-01-14" }
        };
    }
}
