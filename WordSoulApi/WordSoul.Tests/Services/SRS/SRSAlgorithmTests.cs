

using FluentAssertions;
using WordSoul.Application.Services.SRS;

namespace WordSoul.Tests.Services.SRS
{
    /// <summary>
    /// Unit tests cho SRSAlgorithm
    /// Mỗi test method nên test MỘT behavior cụ thể
    /// </summary>
    public class SRSAlgorithmTests
    {
        // ============ TEST 1: Khởi tạo object ============
        [Fact]  // ← Attribute đánh dấu đây là 1 test method
        public void Constructor_ShouldCreateInstance()
        {
            // ARRANGE (Chuẩn bị)
            // - Không cần chuẩn bị gì

            // ACT (Thực hiện)
            var algorithm = new SRSAlgorithm();

            // ASSERT (Kiểm tra kết quả)
            algorithm.Should().NotBeNull();  // FluentAssertions syntax
            // Hoặc dùng xUnit syntax: Assert.NotNull(algorithm);
        }

        [Fact]
        public void CalculateNext_WithGrade5_ShouldIncreaseEFAndInterval()
        {
            // ARRANGE (Chuẩn bị input)
            var algorithm = new SRSAlgorithm();
            int grade = 5;                    // Perfect recall
            double currentEF = 2.5;           // Default EF
            int currentInterval = 6;          // Đã qua 6 ngày
            int currentRepetition = 2;        // Đã nhớ đúng 2 lần

            // ACT (Gọi method cần test)
            var result = algorithm.CalculateNext(
                grade,
                currentEF,
                currentInterval,
                currentRepetition
            );

            // ASSERT (Kiểm tra kết quả)
            // 1. EF phải tăng (vì grade = 5)
            result.NewEaseFactor.Should().BeGreaterThan(currentEF);

            // 2. Repetition phải tăng lên 1
            result.NewRepetition.Should().Be(currentRepetition + 1);

            // 3. Interval phải được tính đúng: I(n) = I(n-1) × EF
            // Với rep=3, interval = 6 × 2.6 = 15.6 → Math.Ceiling = 16
            result.NewInterval.Should().BeGreaterThan(currentInterval);

            // 4. NextReviewDate phải là tương lai
            result.NextReviewDate.Should().BeAfter(DateTime.UtcNow);

            // 5. MemoryState = "Review" (chưa Mastered vì interval < 21)
            result.MemoryState.Should().Be("Review");
        }


        [Fact]
        public void CalculateNext_WithGrade0_ShouldResetRepetitionAndDecreaseEF()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            int grade = 0;                    // Complete forget
            double currentEF = 2.5;
            int currentInterval = 15;         // Đã ở interval cao
            int currentRepetition = 5;        // Đã nhớ đúng 5 lần trước đó

            // ACT
            var result = algorithm.CalculateNext(
                grade,
                currentEF,
                currentInterval,
                currentRepetition
            );

            // ASSERT
            // 1. EF phải giảm (vì grade = 0)
            result.NewEaseFactor.Should().BeLessThan(currentEF);

            // 2. Repetition phải RESET về 0 (quên rồi!)
            result.NewRepetition.Should().Be(0);

            // 3. Interval phải về 0 (phải ôn lại ngay)
            result.NewInterval.Should().Be(0);

            // 4. MemoryState = "Relearning"
            result.MemoryState.Should().Be("Relearning");
        }

        [Fact]
        public void CalculateNext_FirstTimeCorrect_ShouldSetIntervalToOneDay()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            int grade = 4;                    // Good recall
            double currentEF = 2.5;
            int currentInterval = 0;          // Chưa từng ôn
            int currentRepetition = 0;        // Lần đầu tiên

            // ACT
            var result = algorithm.CalculateNext(
                grade,
                currentEF,
                currentInterval,
                currentRepetition
            );

            // ASSERT
            // Theo SM-2: Lần đầu đúng → interval = 1 day
            result.NewRepetition.Should().Be(1);
            result.NewInterval.Should().Be(1);
            result.MemoryState.Should().Be("Learning");
        }


        [Fact]
        public void CalculateNext_SecondReview_ShouldSetIntervalToSixDays()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            int grade = 3;                    // Good (minimum passing)
            double currentEF = 2.5;
            int currentInterval = 1;          // Sau lần đầu
            int currentRepetition = 1;        // Đã nhớ 1 lần

            // ACT
            var result = algorithm.CalculateNext(
                grade,
                currentEF,
                currentInterval,
                currentRepetition
            );

            // ASSERT
            // Theo SM-2: Lần thứ 2 đúng → interval = 6 days (hardcoded)
            result.NewRepetition.Should().Be(2);
            result.NewInterval.Should().Be(6);
        }

        [Theory]
        [InlineData(5, 2.5, 6, 2, 2.6, 16, 3)]
        [InlineData(4, 2.5, 6, 2, 2.5, 15, 3)]
        [InlineData(3, 2.5, 6, 2, 2.36, 15, 3)]
        [InlineData(2, 2.5, 6, 2, 2.18, 0, 0)]
        [InlineData(1, 2.5, 6, 2, 1.96, 0, 0)]
        [InlineData(0, 2.5, 6, 2, 1.7, 0, 0)]
        public void CalculateNext_WithVariousGrades_ShouldCalculateCorrectly(
            int grade,
            double currentEF,
            int currentInterval,
            int currentRep,
            double expectedEF,      // ← Expected values
            int expectedInterval,
            int expectedRep)
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();

            // ACT
            var result = algorithm.CalculateNext(
                grade,
                currentEF,
                currentInterval,
                currentRep
            );

            // ASSERT
            result.NewEaseFactor.Should().BeApproximately(expectedEF, 0.01);  // ±0.01 tolerance
            result.NewInterval.Should().Be(expectedInterval);
            result.NewRepetition.Should().Be(expectedRep);
        }


        [Theory]
        [InlineData(-1)]   // Grade < 0
        [InlineData(6)]    // Grade > 5
        [InlineData(10)]   // Way out of range
        public void CalculateNext_WithInvalidGrade_ShouldThrowException(int invalidGrade)
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();

            // ACT & ASSERT (combined)
            Action act = () => algorithm.CalculateNext(
                invalidGrade,
                2.5,
                6,
                2
            );

            // Verify exception is thrown
            act.Should().Throw<ArgumentException>()
               .WithMessage("*Grade must be 0-5*");  // * = wildcard
        }

        [Fact]
        public void CalculateNext_WithLowEF_ShouldNotGoBelowMinimum()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            int grade = 0;                    // Worst grade
            double currentEF = 1.3;           // Already at minimum

            // ACT
            var result = algorithm.CalculateNext(grade, currentEF, 6, 2);

            // ASSERT
            // EF không được xuống dưới 1.3
            result.NewEaseFactor.Should().BeGreaterThanOrEqualTo(1.3);
        }

        [Fact]
        public void CalculateNext_WithHighEF_ShouldNotExceedMaximum()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            int grade = 5;                    // Best grade
            double currentEF = 3.9;           // Almost at maximum

            // ACT
            var result = algorithm.CalculateNext(grade, currentEF, 6, 2);

            // ASSERT
            // EF không được vượt 4.0
            result.NewEaseFactor.Should().BeLessThanOrEqualTo(4.0);
        }

        [Theory]
        [InlineData(0, 0, "Relearning")]
        [InlineData(1, 1, "Learning")]
        [InlineData(2, 6, "Learning")]
        [InlineData(3, 15, "Review")]
        [InlineData(5, 21, "Mastered")]    // 21 days = threshold
        [InlineData(10, 100, "Mastered")]  // Long interval
        public void DetermineMemoryState_WithVariousInputs_ShouldReturnCorrectState(
            int repetition,
            int interval,
            string expectedState)
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();

            // ACT
            var result = algorithm.GetMemoryState(repetition, interval);

            // ASSERT
            result.Should().Be(expectedState);
        }



        [Theory]
        [InlineData(10, 0, 0, 100)]     // Perfect accuracy, no repetition
        [InlineData(8, 2, 0, 80)]       // 80% accuracy
        [InlineData(10, 0, 5, 100)]     // Perfect + 10 bonus (capped at 100)
        [InlineData(8, 2, 10, 100)]     // 80% + 20 bonus = 100 (capped)
        [InlineData(5, 5, 3, 56)]       // 50% + 6 bonus = 56%
        [InlineData(0, 0, 0, 0)]        // No attempts = 0
        public void CalculateRetentionScore_WithVariousInputs_ShouldCalculateCorrectly(
            int correctCount,
            int wrongCount,
            int repetition,
            decimal expectedScore)
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();

            // ACT
            var score = algorithm.CalculateRetentionScore(
                correctCount,
                wrongCount,
                repetition
            );

            // ASSERT
            score.Should().Be(expectedScore);
        }
    }
}
