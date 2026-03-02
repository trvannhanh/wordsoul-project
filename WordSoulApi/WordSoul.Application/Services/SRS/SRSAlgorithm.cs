using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordSoul.Application.DTOs.SRS;

namespace WordSoul.Application.Services.SRS
{
    /// <summary>
    /// SM-2 (SuperMemo 2) Algorithm Implementation
    /// Reference: https://www.supermemo.com/en/archives1990-2015/english/ol/sm2
    /// </summary>
    public class SRSAlgorithm
    {
        // Constants
        private const double MIN_EASE_FACTOR = 1.3;
        private const double DEFAULT_EASE_FACTOR = 2.5;
        private const double MAX_EASE_FACTOR = 4.0;

        /// <summary>
        /// Calculate next review parameters based on SM-2
        /// </summary>
        /// <param name="grade">Quality of recall (0-5)</param>
        /// <param name="currentEF">Current Easiness Factor</param>
        /// <param name="currentInterval">Current interval in days</param>
        /// <param name="currentRepetition">Current repetition count</param>
        /// <returns>Updated SRS parameters</returns>
        public SRSResult CalculateNext(
            int grade,
            double currentEF,
            int currentInterval,
            int currentRepetition)
        {
            // Validate input
            if (grade < 0 || grade > 5)
                throw new ArgumentException("Grade must be 0-5", nameof(grade));

            var result = new SRSResult();

            // Step 1: Calculate new Easiness Factor
            result.NewEaseFactor = CalculateNewEaseFactor(grade, currentEF);

            // Step 2: Update Repetition count
            if (grade >= 3)  // Passed (Good or better)
            {
                result.NewRepetition = currentRepetition + 1;
            }
            else  // Failed or struggled
            {
                result.NewRepetition = 0;  // Reset to start
            }

            // Step 3: Calculate new Interval
            result.NewInterval = CalculateNewInterval(
                result.NewRepetition,
                currentInterval,
                result.NewEaseFactor
            );

            // Step 4: Calculate next review date
            result.NextReviewDate = DateTime.UtcNow.AddDays(result.NewInterval);

            // Step 5: Determine memory state
            result.MemoryState = DetermineMemoryState(
                result.NewRepetition,
                result.NewInterval
            );

            return result;
        }

        /// <summary>
        /// SM-2 Easiness Factor calculation
        /// EF' = EF + (0.1 - (5-q) * (0.08 + (5-q) * 0.02))
        /// </summary>
        private double CalculateNewEaseFactor(int grade, double currentEF)
        {
            // SM-2 formula
            double change = 0.1 - (5 - grade) * (0.08 + (5 - grade) * 0.02);
            double newEF = currentEF + change;

            // Clamp between min and max
            return Math.Max(MIN_EASE_FACTOR, Math.Min(MAX_EASE_FACTOR, newEF));
        }

        /// <summary>
        /// SM-2 Interval calculation
        /// </summary>
        private int CalculateNewInterval(int repetition, int currentInterval, double easeFactor)
        {
            switch (repetition)
            {
                case 0:
                    return 0;  // Review immediately (same day or next session)

                case 1:
                    return 1;  // Review after 1 day

                case 2:
                    return 6;  // Review after 6 days

                default:
                    // I(n) = I(n-1) * EF
                    return (int)Math.Ceiling(currentInterval * easeFactor);
            }
        }

        /// <summary>
        /// Determine memory state based on repetitions and interval
        /// </summary>
        private string DetermineMemoryState(int repetition, int interval)
        {
            if (repetition == 0)
                return "Relearning";  // Failed → need to relearn

            if (repetition <= 2)
                return "Learning";  // Just started

            if (interval >= 21)
                return "Mastered";  // 3+ weeks → long-term memory

            return "Review";  // In progress
        }

        /// <summary>
        /// Calculate retention score (0-100%)
        /// Combines accuracy with repetition strength
        /// </summary>
        public decimal CalculateRetentionScore(
            int correctCount,
            int wrongCount,
            int repetition)
        {
            if (correctCount + wrongCount == 0)
                return 0;

            // Base accuracy (0-100)
            decimal accuracy = (decimal)correctCount / (correctCount + wrongCount) * 100;

            // Bonus for consecutive correct recalls (max +20)
            decimal repetitionBonus = Math.Min(repetition * 2, 20);

            // Combined score (capped at 100)
            return Math.Min(accuracy + repetitionBonus, 100);
        }

        public string GetMemoryState(int repetition, int interval)
        {
            return DetermineMemoryState(repetition, interval);
        }
    }

    
}
