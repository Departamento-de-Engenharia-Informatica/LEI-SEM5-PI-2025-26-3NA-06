using FluentAssertions;
using ProjArqsi.Domain.VesselVisitNotificationAggregate;

namespace Backend.Tests.ValueObjectTests.VesselVisitNotification
{
    public class CrewMembersListTests
    {
        #region Valid CrewMembersList Tests

        [Fact]
        public void Constructor_WithEmptyList_ShouldCreateCrewMembersList()
        {
            // Arrange
            var emptyList = new List<CrewMember>();

            // Act
            var crewList = new CrewMembersList(emptyList);

            // Assert
            crewList.Should().NotBeNull();
            crewList.Members.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithNull_ShouldCreateEmptyCrewMembersList()
        {
            // Act
            var crewList = new CrewMembersList(null);

            // Assert
            crewList.Should().NotBeNull();
            crewList.Members.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_WithSingleCrewMember_ShouldCreateCrewMembersList()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain)
            };

            // Act
            var crewList = new CrewMembersList(crew);

            // Assert
            crewList.Members.Should().HaveCount(1);
            crewList.Members.First().Name.Should().Be("John Smith");
        }

        [Fact]
        public void Constructor_WithMultipleCrewMembers_ShouldCreateCrewMembersList()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain),
                new CrewMember("Maria Garcia", "B789012", "Spain", CrewMemberRole.SafetyOfficer),
                new CrewMember("Li Wei", "C345678", "China", CrewMemberRole.CrewMember)
            };

            // Act
            var crewList = new CrewMembersList(crew);

            // Assert
            crewList.Members.Should().HaveCount(3);
        }

        #endregion

        #region HasCaptain Tests

        [Fact]
        public void HasCaptain_WithCaptainInCrew_ShouldReturnTrue()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain),
                new CrewMember("Jane Doe", "B789012", "UK", CrewMemberRole.CrewMember)
            };
            var crewList = new CrewMembersList(crew);

            // Act
            var result = crewList.HasCaptain();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasCaptain_WithoutCaptain_ShouldReturnFalse()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("Jane Doe", "B789012", "UK", CrewMemberRole.CrewMember),
                new CrewMember("Bob Jones", "C345678", "Canada", CrewMemberRole.SafetyOfficer)
            };
            var crewList = new CrewMembersList(crew);

            // Act
            var result = crewList.HasCaptain();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasCaptain_WithEmptyList_ShouldReturnFalse()
        {
            // Arrange
            var crewList = new CrewMembersList(null);

            // Act
            var result = crewList.HasCaptain();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region HasSafetyOfficer Tests

        [Fact]
        public void HasSafetyOfficer_WithSafetyOfficerInCrew_ShouldReturnTrue()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain),
                new CrewMember("Maria Garcia", "B789012", "Spain", CrewMemberRole.SafetyOfficer)
            };
            var crewList = new CrewMembersList(crew);

            // Act
            var result = crewList.HasSafetyOfficer();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void HasSafetyOfficer_WithoutSafetyOfficer_ShouldReturnFalse()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain),
                new CrewMember("Jane Doe", "B789012", "UK", CrewMemberRole.CrewMember)
            };
            var crewList = new CrewMembersList(crew);

            // Act
            var result = crewList.HasSafetyOfficer();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void HasSafetyOfficer_WithEmptyList_ShouldReturnFalse()
        {
            // Arrange
            var crewList = new CrewMembersList(null);

            // Act
            var result = crewList.HasSafetyOfficer();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoCrewMembersListsWithSameMembers_ShouldBeEqual()
        {
            // Arrange
            var crew1 = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain)
            };
            var crew2 = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain)
            };
            var crewList1 = new CrewMembersList(crew1);
            var crewList2 = new CrewMembersList(crew2);

            // Act & Assert
            crewList1.Equals(crewList2).Should().BeTrue();
            crewList1.GetHashCode().Should().Be(crewList2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoCrewMembersListsWithDifferentMembers_ShouldNotBeEqual()
        {
            // Arrange
            var crew1 = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain)
            };
            var crew2 = new List<CrewMember>
            {
                new CrewMember("Jane Doe", "B789012", "UK", CrewMemberRole.Captain)
            };
            var crewList1 = new CrewMembersList(crew1);
            var crewList2 = new CrewMembersList(crew2);

            // Act & Assert
            crewList1.Equals(crewList2).Should().BeFalse();
        }

        [Fact]
        public void Equality_TwoEmptyCrewMembersLists_ShouldBeEqual()
        {
            // Arrange
            var crewList1 = new CrewMembersList(null);
            var crewList2 = new CrewMembersList(new List<CrewMember>());

            // Act & Assert
            crewList1.Equals(crewList2).Should().BeTrue();
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Members_ShouldBeReadOnly()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain)
            };
            var crewList = new CrewMembersList(crew);

            // Act
            var members = crewList.Members;

            // Assert
            members.Should().BeAssignableTo<IReadOnlyCollection<CrewMember>>();
        }

        [Fact]
        public void Constructor_WithLargeCrewList_ShouldCreateCrewMembersList()
        {
            // Arrange - Large crew (e.g., cruise ship)
            var crew = Enumerable.Range(1, 100)
                .Select(i => new CrewMember($"Crew {i}", $"ID{i:D6}", "Various", CrewMemberRole.CrewMember))
                .ToList();

            // Act
            var crewList = new CrewMembersList(crew);

            // Assert
            crewList.Members.Should().HaveCount(100);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_SmallVesselCrew_ShouldCreateCrewMembersList()
        {
            // Arrange - Small cargo vessel with minimal crew
            var crew = new List<CrewMember>
            {
                new CrewMember("James Wilson", "UK123456", "United Kingdom", CrewMemberRole.Captain),
                new CrewMember("Thomas Brown", "UK654321", "United Kingdom", CrewMemberRole.SafetyOfficer),
                new CrewMember("Robert Taylor", "UK789012", "United Kingdom", CrewMemberRole.CrewMember),
                new CrewMember("Michael Davis", "UK345678", "United Kingdom", CrewMemberRole.CrewMember)
            };

            // Act
            var crewList = new CrewMembersList(crew);

            // Assert
            crewList.Members.Should().HaveCount(4);
            crewList.HasCaptain().Should().BeTrue();
            crewList.HasSafetyOfficer().Should().BeTrue();
        }

        [Fact]
        public void Constructor_InternationalCrew_ShouldCreateCrewMembersList()
        {
            // Arrange - International crew from multiple countries
            var crew = new List<CrewMember>
            {
                new CrewMember("Giovanni Rossi", "IT987654", "Italy", CrewMemberRole.Captain),
                new CrewMember("Hans Mueller", "DE456789", "Germany", CrewMemberRole.SafetyOfficer),
                new CrewMember("Pierre Dubois", "FR123456", "France", CrewMemberRole.CrewMember),
                new CrewMember("Maria Silva", "PT789012", "Portugal", CrewMemberRole.CrewMember),
                new CrewMember("Jan Kowalski", "PL345678", "Poland", CrewMemberRole.CrewMember)
            };

            // Act
            var crewList = new CrewMembersList(crew);

            // Assert
            crewList.Members.Should().HaveCount(5);
            crewList.Members.Select(m => m.Nationality).Distinct().Should().HaveCount(5);
        }

        [Fact]
        public void HasCaptain_ContainerVesselWithFullCrew_ShouldHaveCaptain()
        {
            // Arrange
            var crew = new List<CrewMember>
            {
                new CrewMember("Captain Smith", "A123456", "USA", CrewMemberRole.Captain),
                new CrewMember("Chief Officer Jones", "B789012", "USA", CrewMemberRole.SafetyOfficer),
                new CrewMember("Engineer Brown", "C345678", "Canada", CrewMemberRole.CrewMember),
                new CrewMember("Deck Officer White", "D901234", "UK", CrewMemberRole.CrewMember)
            };
            var crewList = new CrewMembersList(crew);

            // Act & Assert
            crewList.HasCaptain().Should().BeTrue();
            crewList.HasSafetyOfficer().Should().BeTrue();
        }

        [Fact]
        public void Constructor_EmptyVesselForMaintenance_ShouldAllowEmptyCrew()
        {
            // Arrange - Vessel arriving empty for maintenance
            var crewList = new CrewMembersList(null);

            // Act & Assert
            crewList.Members.Should().BeEmpty();
            crewList.HasCaptain().Should().BeFalse();
        }

        #endregion
    }

    public class CrewMemberTests
    {
        #region Valid CrewMember Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateCrewMember()
        {
            // Arrange & Act
            var crewMember = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain);

            // Assert
            crewMember.Should().NotBeNull();
            crewMember.Name.Should().Be("John Smith");
            crewMember.CitizenId.Should().Be("A123456");
            crewMember.Nationality.Should().Be("USA");
            crewMember.Role.Should().Be(CrewMemberRole.Captain);
        }

        [Theory]
        [InlineData("Maria Garcia", "B789012", "Spain", CrewMemberRole.SafetyOfficer)]
        [InlineData("Li Wei", "C345678", "China", CrewMemberRole.CrewMember)]
        [InlineData("Ahmed Hassan", "D901234", "Egypt", CrewMemberRole.CrewMember)]
        public void Constructor_WithVariousValidParameters_ShouldCreateCrewMember(
            string name, string citizenId, string nationality, CrewMemberRole role)
        {
            // Act
            var crewMember = new CrewMember(name, citizenId, nationality, role);

            // Assert
            crewMember.Name.Should().Be(name);
            crewMember.CitizenId.Should().Be(citizenId);
            crewMember.Nationality.Should().Be(nationality);
            crewMember.Role.Should().Be(role);
        }

        [Fact]
        public void Constructor_WithWhitespaceInName_ShouldTrimWhitespace()
        {
            // Arrange & Act
            var crewMember = new CrewMember("  John Smith  ", "A123456", "USA", CrewMemberRole.Captain);

            // Assert
            crewMember.Name.Should().Be("John Smith");
        }

        [Fact]
        public void Constructor_WithWhitespaceInCitizenId_ShouldTrimWhitespace()
        {
            // Arrange & Act
            var crewMember = new CrewMember("John Smith", "  A123456  ", "USA", CrewMemberRole.Captain);

            // Assert
            crewMember.CitizenId.Should().Be("A123456");
        }

        [Fact]
        public void Constructor_WithWhitespaceInNationality_ShouldTrimWhitespace()
        {
            // Arrange & Act
            var crewMember = new CrewMember("John Smith", "A123456", "  USA  ", CrewMemberRole.Captain);

            // Assert
            crewMember.Nationality.Should().Be("USA");
        }

        #endregion

        #region Null/Empty Parameter Tests

        [Fact]
        public void Constructor_WithNullName_ShouldCreateCrewMemberWithEmptyName()
        {
            // Act
            var crewMember = new CrewMember(null!, "A123456", "USA", CrewMemberRole.Captain);

            // Assert
            crewMember.Name.Should().Be(string.Empty);
        }

        [Fact]
        public void Constructor_WithNullCitizenId_ShouldCreateCrewMemberWithEmptyCitizenId()
        {
            // Act
            var crewMember = new CrewMember("John Smith", null!, "USA", CrewMemberRole.Captain);

            // Assert
            crewMember.CitizenId.Should().Be(string.Empty);
        }

        [Fact]
        public void Constructor_WithNullNationality_ShouldCreateCrewMemberWithEmptyNationality()
        {
            // Act
            var crewMember = new CrewMember("John Smith", "A123456", null!, CrewMemberRole.Captain);

            // Assert
            crewMember.Nationality.Should().Be(string.Empty);
        }

        [Fact]
        public void Constructor_WithEmptyStrings_ShouldCreateCrewMember()
        {
            // Act
            var crewMember = new CrewMember("", "", "", CrewMemberRole.CrewMember);

            // Assert
            crewMember.Name.Should().Be(string.Empty);
            crewMember.CitizenId.Should().Be(string.Empty);
            crewMember.Nationality.Should().Be(string.Empty);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equality_TwoCrewMembersWithSameValues_ShouldBeEqual()
        {
            // Arrange
            var crewMember1 = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain);
            var crewMember2 = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain);

            // Act & Assert
            crewMember1.Equals(crewMember2).Should().BeTrue();
            crewMember1.GetHashCode().Should().Be(crewMember2.GetHashCode());
        }

        [Fact]
        public void Equality_TwoCrewMembersWithDifferentNames_ShouldNotBeEqual()
        {
            // Arrange
            var crewMember1 = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain);
            var crewMember2 = new CrewMember("Jane Doe", "A123456", "USA", CrewMemberRole.Captain);

            // Act & Assert
            crewMember1.Equals(crewMember2).Should().BeFalse();
        }

        [Fact]
        public void Equality_TwoCrewMembersWithDifferentRoles_ShouldNotBeEqual()
        {
            // Arrange
            var crewMember1 = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain);
            var crewMember2 = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.CrewMember);

            // Act & Assert
            crewMember1.Equals(crewMember2).Should().BeFalse();
        }

        #endregion

        #region CrewMemberRole Enum Tests

        [Fact]
        public void CrewMemberRole_ShouldHaveExpectedValues()
        {
            // Assert
            Enum.IsDefined(typeof(CrewMemberRole), CrewMemberRole.Captain).Should().BeTrue();
            Enum.IsDefined(typeof(CrewMemberRole), CrewMemberRole.SafetyOfficer).Should().BeTrue();
            Enum.IsDefined(typeof(CrewMemberRole), CrewMemberRole.CrewMember).Should().BeTrue();
        }

        [Theory]
        [InlineData(CrewMemberRole.Captain)]
        [InlineData(CrewMemberRole.SafetyOfficer)]
        [InlineData(CrewMemberRole.CrewMember)]
        public void Constructor_WithAllRoles_ShouldCreateCrewMember(CrewMemberRole role)
        {
            // Act
            var crewMember = new CrewMember("Test Name", "TEST123", "Test", role);

            // Assert
            crewMember.Role.Should().Be(role);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void CrewMember_ShouldBeImmutable()
        {
            // Arrange
            var crewMember = new CrewMember("John Smith", "A123456", "USA", CrewMemberRole.Captain);

            // Act & Assert - Properties should not have public setters
            crewMember.Name.Should().Be("John Smith");
            crewMember.CitizenId.Should().Be("A123456");
            crewMember.Nationality.Should().Be("USA");
            crewMember.Role.Should().Be(CrewMemberRole.Captain);
        }

        [Fact]
        public void Constructor_WithLongName_ShouldCreateCrewMember()
        {
            // Arrange
            var longName = "Captain Jean-Luc Pierre François Alexandre de la Fontaine";

            // Act
            var crewMember = new CrewMember(longName, "FR123456", "France", CrewMemberRole.Captain);

            // Assert
            crewMember.Name.Should().Be(longName);
        }

        [Fact]
        public void Constructor_WithUnicodeCharacters_ShouldPreserveCharacters()
        {
            // Arrange
            var unicodeName = "José María García";
            var unicodeNationality = "España";

            // Act
            var crewMember = new CrewMember(unicodeName, "ES123456", unicodeNationality, CrewMemberRole.Captain);

            // Assert
            crewMember.Name.Should().Be(unicodeName);
            crewMember.Nationality.Should().Be(unicodeNationality);
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Constructor_CaptainWithFullDetails_ShouldCreateCrewMember()
        {
            // Arrange & Act
            var captain = new CrewMember(
                "Captain James T. Kirk",
                "USA-1701",
                "United States",
                CrewMemberRole.Captain);

            // Assert
            captain.Name.Should().Be("Captain James T. Kirk");
            captain.Role.Should().Be(CrewMemberRole.Captain);
        }

        [Fact]
        public void Constructor_SafetyOfficerWithFullDetails_ShouldCreateCrewMember()
        {
            // Arrange & Act
            var safetyOfficer = new CrewMember(
                "Maria Garcia",
                "ES789012",
                "Spain",
                CrewMemberRole.SafetyOfficer);

            // Assert
            safetyOfficer.Role.Should().Be(CrewMemberRole.SafetyOfficer);
        }

        [Fact]
        public void Constructor_RegularCrewMemberWithFullDetails_ShouldCreateCrewMember()
        {
            // Arrange & Act
            var regularCrew = new CrewMember(
                "Li Wei",
                "CN345678",
                "China",
                CrewMemberRole.CrewMember);

            // Assert
            regularCrew.Role.Should().Be(CrewMemberRole.CrewMember);
        }

        #endregion
    }
}
