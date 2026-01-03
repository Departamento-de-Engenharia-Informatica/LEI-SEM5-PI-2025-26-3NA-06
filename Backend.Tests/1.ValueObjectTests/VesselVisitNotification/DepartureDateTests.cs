using FluentAssertions;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class DepartureDateTests
    {
        #region Valid DepartureDate Tests

        [Fact]
        public void Constructor_WithValidDate_ShouldCreateDepartureDate()
        {
            // Arrange
            var date = DateTime.Now.AddDays(14);

            // Act
            var departureDate = new DepartureDate(date);

            // Assert
            departureDate.Should().NotBeNull();
            departureDate.Value.Should().Be(date);
        }

        [Fact]
        public void Constructor_WithNull_ShouldCreateDepartureDateWithNullValue()
        {
            // Act
            var departureDate = new DepartureDate(null);

            // Assert
            departureDate.Should().NotBeNull();
            departureDate.Value.Should().BeNull();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(30)]
        [InlineData(365)]
        public void Constructor_WithFutureDates_ShouldCreateDepartureDate(int daysInFuture)
        {
            // Arrange
            var date = DateTime.Now.AddDays(daysInFuture);

            // Act
            var departureDate = new DepartureDate(date);

            // Assert
            departureDate.Value.Should().Be(date);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-7)]
        [InlineData(-30)]
        public void Constructor_WithPastDates_ShouldCreateDepartureDate(int daysInPast)
        {
            // Arrange
            var date = DateTime.Now.AddDays(daysInPast);

            // Act
            var departureDate = new DepartureDate(date);

            // Assert
            departureDate.Value.Should().Be(date);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoDepartureDatesWithSameDate_ShouldBeEqual()
        {
            // Arrange
            var date = new DateTime(2024, 12, 26, 10, 0, 0);
            var departureDate1 = new DepartureDate(date);
            var departureDate2 = new DepartureDate(date);

            // Act & Assert
            departureDate1.Equals(departureDate2).Should().BeTrue();
            departureDate1.GetHashCode().Should().Be(departureDate2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoDepartureDatesWithDifferentDates_ShouldNotBeEqual()
        {
            // Arrange
            var date1 = new DateTime(2024, 12, 26);
            var date2 = new DateTime(2024, 12, 27);
            var departureDate1 = new DepartureDate(date1);
            var departureDate2 = new DepartureDate(date2);

            // Act & Assert
            departureDate1.Equals(departureDate2).Should().BeFalse();
        }

        [Fact]
        public void Equality_TwoDepartureDatesWithNull_ShouldBeEqual()
        {
            // Arrange
            var departureDate1 = new DepartureDate(null);
            var departureDate2 = new DepartureDate(null);

            // Act & Assert
            departureDate1.Equals(departureDate2).Should().BeTrue();
            departureDate1.GetHashCode().Should().Be(departureDate2.GetHashCode());
        }

        [Fact]
        public void Equality_DepartureDateWithDateAndDepartureDateWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var date = DateTime.Now.AddDays(14);
            var departureDate1 = new DepartureDate(date);
            var departureDate2 = new DepartureDate(null);

            // Act & Assert
            departureDate1.Equals(departureDate2).Should().BeFalse();
        }

        [Fact]
        public void Equality_DepartureDateComparedWithNull_ShouldNotBeEqual()
        {
            // Arrange
            var date = DateTime.Now.AddDays(14);
            var departureDate = new DepartureDate(date);

            // Act & Assert
            departureDate.Equals(null).Should().BeFalse();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithMinDate_ShouldCreateDepartureDate()
        {
            // Arrange
            var minDate = DateTime.MinValue;

            // Act
            var departureDate = new DepartureDate(minDate);

            // Assert
            departureDate.Value.Should().Be(minDate);
        }

        [Fact]
        public void Constructor_WithMaxDate_ShouldCreateDepartureDate()
        {
            // Arrange
            var maxDate = DateTime.MaxValue;

            // Act
            var departureDate = new DepartureDate(maxDate);

            // Assert
            departureDate.Value.Should().Be(maxDate);
        }

        [Fact]
        public void Constructor_WithDateTimeWithMilliseconds_ShouldPreserveExactValue()
        {
            // Arrange
            var date = new DateTime(2024, 12, 26, 16, 45, 30, 789);

            // Act
            var departureDate = new DepartureDate(date);

            // Assert
            departureDate.Value.Should().Be(date);
            departureDate.Value?.Millisecond.Should().Be(789);
        }

        [Fact]
        public void DepartureDate_ShouldBeImmutable()
        {
            // Arrange
            var date = DateTime.Now.AddDays(14);
            var departureDate = new DepartureDate(date);
            var originalValue = departureDate.Value;

            // Act - Try to modify through reference (should not affect original)
            date = date.AddDays(1);

            // Assert
            departureDate.Value.Should().Be(originalValue);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_WithTodayDate_ShouldCreateDepartureDate()
        {
            // Arrange
            var today = DateTime.Today;

            // Act
            var departureDate = new DepartureDate(today);

            // Assert
            departureDate.Value.Should().Be(today);
        }

        [Fact]
        public void Constructor_WithSpecificTimeOfDay_ShouldPreserveTime()
        {
            // Arrange - Late evening departure
            var lateEvening = new DateTime(2024, 12, 26, 22, 0, 0);

            // Act
            var departureDate = new DepartureDate(lateEvening);

            // Assert
            departureDate.Value.Should().Be(lateEvening);
            departureDate.Value?.Hour.Should().Be(22);
        }

        [Fact]
        public void Constructor_WithUtcDate_ShouldPreserveDateKind()
        {
            // Arrange
            var utcDate = DateTime.UtcNow.AddDays(14);

            // Act
            var departureDate = new DepartureDate(utcDate);

            // Assert
            departureDate.Value.Should().Be(utcDate);
            departureDate.Value?.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Constructor_WithLocalDate_ShouldPreserveDateKind()
        {
            // Arrange
            var localDate = DateTime.Now.AddDays(14);

            // Act
            var departureDate = new DepartureDate(localDate);

            // Assert
            departureDate.Value.Should().Be(localDate);
            departureDate.Value?.Kind.Should().Be(DateTimeKind.Local);
        }

        [Fact]
        public void Constructor_WithScheduledDepartureInTwoWeeks_ShouldCreateValidDepartureDate()
        {
            // Arrange - Vessel scheduled to depart in exactly 2 weeks
            var scheduledDate = DateTime.Now.AddDays(14);

            // Act
            var departureDate = new DepartureDate(scheduledDate);

            // Assert
            departureDate.Value.Should().Be(scheduledDate);
            departureDate.Value.Should().BeAfter(DateTime.Now);
        }

        [Fact]
        public void Constructor_ForDelayedDeparture_ShouldAllowPastDates()
        {
            // Arrange - Recording a delayed departure that already happened
            var delayedDate = DateTime.Now.AddDays(-1);

            // Act
            var departureDate = new DepartureDate(delayedDate);

            // Assert
            departureDate.Value.Should().Be(delayedDate);
            departureDate.Value.Should().BeBefore(DateTime.Now);
        }

        [Fact]
        public void Constructor_DepartureDateAfterArrival_ShouldBeValid()
        {
            // Arrange - Typical scenario: arrival on Dec 20, departure on Dec 25
            var arrivalDate = new DateTime(2024, 12, 20, 8, 0, 0);
            var departureDateTime = new DateTime(2024, 12, 25, 18, 0, 0);

            // Act
            var departureDate = new DepartureDate(departureDateTime);

            // Assert
            departureDate.Value.Should().BeAfter(arrivalDate);
        }

        [Fact]
        public void Constructor_ShortPortStay_ShouldBeValid()
        {
            // Arrange - Quick turnaround: same day departure (8 hours after arrival)
            var arrivalDateTime = new DateTime(2024, 12, 20, 6, 0, 0);
            var departureDateTime = new DateTime(2024, 12, 20, 14, 0, 0);

            // Act
            var departureDate = new DepartureDate(departureDateTime);

            // Assert
            departureDate.Value.Should().BeAfter(arrivalDateTime);
            departureDate.Value.Should().BeSameDateAs(arrivalDateTime);
        }

        #endregion
    }
}
