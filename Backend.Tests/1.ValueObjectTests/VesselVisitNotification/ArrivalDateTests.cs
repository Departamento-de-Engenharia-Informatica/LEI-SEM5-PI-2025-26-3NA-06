using FluentAssertions;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class ArrivalDateTests
    {
        #region Valid ArrivalDate Tests

        [Fact]
        public void Constructor_WithValidDate_ShouldCreateArrivalDate()
        {
            // Arrange
            var date = DateTime.Now.AddDays(7);

            // Act
            var arrivalDate = new ArrivalDate(date);

            // Assert
            arrivalDate.Should().NotBeNull();
            arrivalDate.Value.Should().Be(date);
        }

        [Fact]
        public void Constructor_WithNull_ShouldCreateArrivalDateWithNullValue()
        {
            // Act
            var arrivalDate = new ArrivalDate(null);

            // Assert
            arrivalDate.Should().NotBeNull();
            arrivalDate.Value.Should().BeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(365)]
        public void Constructor_WithFutureDates_ShouldCreateArrivalDate(int daysInFuture)
        {
            // Arrange
            var date = DateTime.Now.AddDays(daysInFuture);

            // Act
            var arrivalDate = new ArrivalDate(date);

            // Assert
            arrivalDate.Value.Should().Be(date);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-7)]
        [InlineData(-30)]
        public void Constructor_WithPastDates_ShouldCreateArrivalDate(int daysInPast)
        {
            // Arrange
            var date = DateTime.Now.AddDays(daysInPast);

            // Act
            var arrivalDate = new ArrivalDate(date);

            // Assert
            arrivalDate.Value.Should().Be(date);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoArrivalDatesWithSameDate_ShouldBeEqual()
        {
            // Arrange
            var date = new DateTime(2024, 12, 25, 14, 30, 0);
            var arrivalDate1 = new ArrivalDate(date);
            var arrivalDate2 = new ArrivalDate(date);

            // Act & Assert
            arrivalDate1.Equals(arrivalDate2).Should().BeTrue();
            arrivalDate1.GetHashCode().Should().Be(arrivalDate2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoArrivalDatesWithDifferentDates_ShouldNotBeEqual()
        {
            // Arrange
            var date1 = new DateTime(2024, 12, 25);
            var date2 = new DateTime(2024, 12, 26);
            var arrivalDate1 = new ArrivalDate(date1);
            var arrivalDate2 = new ArrivalDate(date2);

            // Act & Assert
            arrivalDate1.Equals(arrivalDate2).Should().BeFalse();
        }

        [Fact]
        public void Equality_TwoArrivalDatesWithNull_ShouldBeEqual()
        {
            // Arrange
            var arrivalDate1 = new ArrivalDate(null);
            var arrivalDate2 = new ArrivalDate(null);

            // Act & Assert
            arrivalDate1.Equals(arrivalDate2).Should().BeTrue();
            arrivalDate1.GetHashCode().Should().Be(arrivalDate2.GetHashCode());
        }

        [Fact]
        public void Equality_ArrivalDateWithDateAndArrivalDateWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var date = DateTime.Now.AddDays(7);
            var arrivalDate1 = new ArrivalDate(date);
            var arrivalDate2 = new ArrivalDate(null);

            // Act & Assert
            arrivalDate1.Equals(arrivalDate2).Should().BeFalse();
        }

        [Fact]
        public void Equality_ArrivalDateComparedWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var date = DateTime.Now.AddDays(7);
            var arrivalDate = new ArrivalDate(date);

            // Act & Assert
            arrivalDate.Equals(null).Should().BeFalse();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMinDate_ShouldCreateArrivalDate()
        {
            // Arrange
            var minDate = DateTime.MinValue;

            // Act
            var arrivalDate = new ArrivalDate(minDate);

            // Assert
            arrivalDate.Value.Should().Be(minDate);
        }

        [Fact]
        public void Constructor_WithMaxDate_ShouldCreateArrivalDate()
        {
            // Arrange
            var maxDate = DateTime.MaxValue;

            // Act
            var arrivalDate = new ArrivalDate(maxDate);

            // Assert
            arrivalDate.Value.Should().Be(maxDate);
        }

        [Fact]
        public void Constructor_WithDateTimeWithMilliseconds_ShouldPreserveExactValue()
        {
            // Arrange
            var date = new DateTime(2024, 12, 25, 14, 30, 45, 123);

            // Act
            var arrivalDate = new ArrivalDate(date);

            // Assert
            arrivalDate.Value.Should().Be(date);
            arrivalDate.Value?.Millisecond.Should().Be(123);
        }

        [Fact]
        public void ArrivalDate_ShouldBeImmutable()
        {
            // Arrange
            var date = DateTime.Now.AddDays(7);
            var arrivalDate = new ArrivalDate(date);
            var originalValue = arrivalDate.Value;

            // Act - Try to modify through reference (should not affect original)
            date = date.AddDays(1);

            // Assert
            arrivalDate.Value.Should().Be(originalValue);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_WithTodayDate_ShouldCreateArrivalDate()
        {
            // Arrange
            var today = DateTime.Today;

            // Act
            var arrivalDate = new ArrivalDate(today);

            // Assert
            arrivalDate.Value.Should().Be(today);
        }

        [Fact]
        public void Constructor_WithSpecificTimeOfDay_ShouldPreserveTime()
        {
            // Arrange - Early morning arrival
            var earlyMorning = new DateTime(2024, 12, 25, 6, 0, 0);

            // Act
            var arrivalDate = new ArrivalDate(earlyMorning);

            // Assert
            arrivalDate.Value.Should().Be(earlyMorning);
            arrivalDate.Value?.Hour.Should().Be(6);
        }

        [Fact]
        public void Constructor_WithUtcDate_ShouldPreserveDateKind()
        {
            // Arrange
            var utcDate = DateTime.UtcNow.AddDays(7);

            // Act
            var arrivalDate = new ArrivalDate(utcDate);

            // Assert
            arrivalDate.Value.Should().Be(utcDate);
            arrivalDate.Value?.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Constructor_WithLocalDate_ShouldPreserveDateKind()
        {
            // Arrange
            var localDate = DateTime.Now.AddDays(7);

            // Act
            var arrivalDate = new ArrivalDate(localDate);

            // Assert
            arrivalDate.Value.Should().Be(localDate);
            arrivalDate.Value?.Kind.Should().Be(DateTimeKind.Local);
        }

        [Fact]
        public void Constructor_WithScheduledArrivalInOneWeek_ShouldCreateValidArrivalDate()
        {
            // Arrange - Vessel scheduled to arrive in exactly 1 week
            var scheduledDate = DateTime.Now.AddDays(7);

            // Act
            var arrivalDate = new ArrivalDate(scheduledDate);

            // Assert
            arrivalDate.Value.Should().Be(scheduledDate);
            arrivalDate.Value.Should().BeAfter(DateTime.Now);
        }

        [Fact]
        public void Constructor_ForDelayedArrival_ShouldAllowPastDates()
        {
            // Arrange - Recording a delayed arrival that already happened
            var delayedDate = DateTime.Now.AddDays(-2);

            // Act
            var arrivalDate = new ArrivalDate(delayedDate);

            // Assert
            arrivalDate.Value.Should().Be(delayedDate);
            arrivalDate.Value.Should().BeBefore(DateTime.Now);
        }

        #endregion
    }
}
