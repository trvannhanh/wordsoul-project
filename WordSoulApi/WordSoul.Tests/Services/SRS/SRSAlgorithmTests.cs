

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

        // ============ TEST: Grade 4 là điểm trung tính của SM-2 ============
        // EF' = EF + (0.1 - (5-4)*(0.08 + (5-4)*0.02)) = EF + (0.1 - 0.10) = EF + 0
        [Fact]
        public void CalculateNext_WithGrade4_EaseFactorShouldRemainUnchanged()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            double currentEF = 2.5;

            // ACT
            var result = algorithm.CalculateNext(grade: 4, currentEF: currentEF, currentInterval: 6, currentRepetition: 2);

            // ASSERT — Grade 4 là điểm trung tính, EF không thay đổi
            result.NewEaseFactor.Should().BeApproximately(currentEF, 0.0001,
                because: "Grade 4 is the neutral point in SM-2 where EF change = 0.1 - 1*(0.08+0.02) = 0");
        }

        // ============ TEST: Công thức interval I(n) = ceil(I(n-1) × EF) ============
        [Fact]
        public void CalculateNext_ThirdRepetitionOnward_IntervalShouldBeCeilingOfPreviousTimesEF()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            double currentEF = 2.5;
            int currentInterval = 10;
            int currentRepetition = 3; // rep >= 3 → dùng công thức EF multiplier

            // ACT
            var result = algorithm.CalculateNext(grade: 4, currentEF: currentEF, currentInterval: currentInterval, currentRepetition: currentRepetition);

            // ASSERT — I(n) = ceil(I(n-1) × NewEF) = ceil(10 × 2.5) = 25
            int expectedInterval = (int)Math.Ceiling(currentInterval * result.NewEaseFactor);
            result.NewInterval.Should().Be(expectedInterval,
                because: $"interval formula is ceil(previousInterval × easeFactor) = ceil({currentInterval} × {result.NewEaseFactor})");
        }

        // ============ TEST: Retention bonus bị cap riêng tại 20 điểm ============
        // Bonus = min(repetition * 2, 20) → repetition >= 10 đều cho bonus = 20
        [Theory]
        [InlineData(10)]   // rep=10 → bonus = min(20, 20) = 20
        [InlineData(15)]   // rep=15 → bonus = min(30, 20) = 20 (capped)
        [InlineData(50)]   // rep=50 → bonus = min(100, 20) = 20 (capped)
        public void CalculateRetentionScore_HighRepetition_BonusShouldBeCappedAt20Points(int highRepetition)
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            // Dùng 70% accuracy để total không bị cap bởi 100: 70 + 20 = 90 < 100
            int correctCount = 7, wrongCount = 3;

            // ACT
            var score = algorithm.CalculateRetentionScore(correctCount, wrongCount, highRepetition);

            // ASSERT — 70% accuracy + 20 bonus (capped) = 90
            score.Should().Be(90,
                because: $"repetition={highRepetition} should still give max bonus of 20, resulting in 70 + 20 = 90");
        }

        // ============ TEST: NextReviewDate phải chính xác với NewInterval ============
        [Fact]
        public void CalculateNext_NextReviewDate_ShouldBeScheduledExactlyIntervalDaysFromNow()
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            var before = DateTime.UtcNow;

            // ACT — grade=4, rep=2 → interval=6 days (hardcoded second repetition)
            var result = algorithm.CalculateNext(grade: 4, currentEF: 2.5, currentInterval: 1, currentRepetition: 1);

            var after = DateTime.UtcNow;

            // ASSERT — NextReviewDate phải nằm trong khoảng before+6days và after+6days
            result.NewInterval.Should().Be(6);
            result.NextReviewDate.Should()
                .BeOnOrAfter(before.AddDays(result.NewInterval))
                .And
                .BeOnOrBefore(after.AddDays(result.NewInterval).AddSeconds(1),
                    because: "NextReviewDate should be exactly UtcNow + NewInterval days");
        }

        // ============ TEST: Boundary grade 2 (fail) vs grade 3 (pass) ============
        // Grade 3 là ngưỡng thấp nhất để PASS (repetition tăng)
        // Grade 2 là ngưỡng cao nhất để FAIL (repetition về 0)
        [Theory]
        [InlineData(3, true,  "should pass threshold and increment repetition")]
        [InlineData(2, false, "should fail threshold and reset repetition to 0")]
        public void CalculateNext_AtPassFailBoundary_ShouldHandleRepetitionCorrectly(
            int grade, bool shouldPass, string reason)
        {
            // ARRANGE
            var algorithm = new SRSAlgorithm();
            int currentRepetition = 4;

            // ACT
            var result = algorithm.CalculateNext(grade: grade, currentEF: 2.5, currentInterval: 10, currentRepetition: currentRepetition);

            // ASSERT
            if (shouldPass)
                result.NewRepetition.Should().Be(currentRepetition + 1, because: reason);
            else
                result.NewRepetition.Should().Be(0, because: reason);
        }

        [Fact]
        public void CalculateNext_CustomSettings_ControlIntervalsAndMastery()
        {
            var algorithm = new SRSAlgorithm();
            var settings = SrsAlgorithmSettings.Default with
            {
                FirstIntervalDays = 2,
                SecondIntervalDays = 8,
                MasteredIntervalDays = 30
            };

            var first = algorithm.CalculateNext(
                grade: 5,
                currentEF: 2.5,
                currentInterval: 0,
                currentRepetition: 0,
                settings: settings);
            var second = algorithm.CalculateNext(
                grade: 5,
                currentEF: first.NewEaseFactor,
                currentInterval: first.NewInterval,
                currentRepetition: first.NewRepetition,
                settings: settings);

            first.NewInterval.Should().Be(2);
            second.NewInterval.Should().Be(8);
            second.MemoryState.Should().Be("Learning");
        }

        [Fact]
        public void CalculateRetentionScore_CustomSettings_ControlRepetitionBonus()
        {
            var algorithm = new SRSAlgorithm();
            var settings = SrsAlgorithmSettings.Default with
            {
                RetentionBonusPerRepetition = 1m,
                RetentionBonusMaximum = 5m
            };

            var score = algorithm.CalculateRetentionScore(
                correctCount: 7,
                wrongCount: 3,
                repetition: 20,
                settings: settings);

            score.Should().Be(75);
        }
    }
}
