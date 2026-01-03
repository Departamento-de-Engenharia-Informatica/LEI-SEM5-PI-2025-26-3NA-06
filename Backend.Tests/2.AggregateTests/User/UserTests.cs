using FluentAssertions;
using ProjArqsi.Domain.Shared;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using Xunit;

namespace Backend.Tests.AggregateTests.User
{
    public class UserTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateUser()
        {
            // Arrange
            var username = new Username("johndoe");
            var role = new Role(RoleType.Admin);
            var email = new Email("john.doe@port.com");

            // Act
            var user = new ProjArqsi.Domain.UserAggregate.User(username, role, email);

            // Assert
            user.Should().NotBeNull();
            user.Id.Should().NotBeNull();
            user.Username.Should().Be(username);
            user.Role.Should().Be(role);
            user.Email.Should().Be(email);
            user.IsActive.Should().BeTrue();
            user.ConfirmationToken.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_ShouldGenerateUniqueIds()
        {
            // Arrange
            var username = new Username("testuser");
            var role = new Role(RoleType.PortAuthorityOfficer);
            var email = new Email("test@port.com");

            // Act
            var user1 = new ProjArqsi.Domain.UserAggregate.User(username, role, email);
            var user2 = new ProjArqsi.Domain.UserAggregate.User(username, role, email);

            // Assert
            user1.Id.Should().NotBe(user2.Id);
        }

        [Fact]
        public void Constructor_WithInactiveFlag_ShouldCreateInactiveUser()
        {
            // Arrange
            var username = new Username("inactiveuser");
            var role = new Role(RoleType.LogisticOperator);
            var email = new Email("inactive@port.com");

            // Act
            var user = new ProjArqsi.Domain.UserAggregate.User(username, role, email, isActive: false);

            // Assert
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Constructor_WithConfirmationToken_ShouldStoreToken()
        {
            // Arrange
            var username = new Username("newuser");
            var role = new Role(RoleType.ShippingAgentRepresentative);
            var email = new Email("new@port.com");
            var token = "abc123token";

            // Act
            var user = new ProjArqsi.Domain.UserAggregate.User(username, role, email, confirmationToken: token);

            // Assert
            user.ConfirmationToken.Should().Be(token);
        }

        [Fact]
        public void Constructor_WithNullUsername_ShouldThrowException()
        {
            // Arrange
            var role = new Role(RoleType.Admin);
            var email = new Email("test@port.com");

            // Act
            var act = () => new ProjArqsi.Domain.UserAggregate.User(null!, role, email);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username is required.");
        }

        [Fact]
        public void Constructor_WithNullRole_ShouldThrowException()
        {
            // Arrange
            var username = new Username("testuser");
            var email = new Email("test@port.com");

            // Act
            var act = () => new ProjArqsi.Domain.UserAggregate.User(username, null!, email);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Role is required.");
        }

        [Fact]
        public void Constructor_WithNullEmail_ShouldThrowException()
        {
            // Arrange
            var username = new Username("testuser");
            var role = new Role(RoleType.Admin);

            // Act
            var act = () => new ProjArqsi.Domain.UserAggregate.User(username, role, null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Email is required.");
        }

        #endregion

        #region Activate/Deactivate Tests

        [Fact]
        public void Activate_ShouldSetIsActiveToTrue()
        {
            // Arrange
            var user = CreateTestUser(isActive: false);

            // Act
            user.Activate();

            // Assert
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_ShouldSetIsActiveToFalse()
        {
            // Arrange
            var user = CreateTestUser(isActive: true);

            // Act
            user.Deactivate();

            // Assert
            user.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldRemainActive()
        {
            // Arrange
            var user = CreateTestUser(isActive: true);

            // Act
            user.Activate();

            // Assert
            user.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_WhenAlreadyInactive_ShouldRemainInactive()
        {
            // Arrange
            var user = CreateTestUser(isActive: false);

            // Act
            user.Deactivate();

            // Assert
            user.IsActive.Should().BeFalse();
        }

        #endregion

        #region ChangeRole Tests

        [Fact]
        public void ChangeRole_WithValidRole_ShouldUpdateRole()
        {
            // Arrange
            var user = CreateTestUser();
            var newRole = new Role(RoleType.LogisticOperator);

            // Act
            user.ChangeRole(newRole);

            // Assert
            user.Role.Should().Be(newRole);
            user.Role.Value.Should().Be(RoleType.LogisticOperator);
        }

        [Fact]
        public void ChangeRole_WithNullRole_ShouldThrowException()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var act = () => user.ChangeRole(null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Role is required.");
        }

        [Fact]
        public void ChangeRole_ShouldPreserveOtherProperties()
        {
            // Arrange
            var user = CreateTestUser();
            var originalUsername = user.Username;
            var originalEmail = user.Email;
            var originalId = user.Id;
            var newRole = new Role(RoleType.ShippingAgentRepresentative);

            // Act
            user.ChangeRole(newRole);

            // Assert
            user.Id.Should().Be(originalId);
            user.Username.Should().Be(originalUsername);
            user.Email.Should().Be(originalEmail);
        }

        #endregion

        #region ChangeUsername Tests

        [Fact]
        public void ChangeUsername_WithValidUsername_ShouldUpdateUsername()
        {
            // Arrange
            var user = CreateTestUser();
            var newUsername = new Username("newusername");

            // Act
            user.ChangeUsername(newUsername);

            // Assert
            user.Username.Should().Be(newUsername);
            user.Username.Value.Should().Be("newusername");
        }

        [Fact]
        public void ChangeUsername_WithNullUsername_ShouldThrowException()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var act = () => user.ChangeUsername(null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Username is required.");
        }

        [Fact]
        public void ChangeUsername_ShouldPreserveOtherProperties()
        {
            // Arrange
            var user = CreateTestUser();
            var originalRole = user.Role;
            var originalEmail = user.Email;
            var originalId = user.Id;
            var newUsername = new Username("differentuser");

            // Act
            user.ChangeUsername(newUsername);

            // Assert
            user.Id.Should().Be(originalId);
            user.Role.Should().Be(originalRole);
            user.Email.Should().Be(originalEmail);
        }

        #endregion

        #region ChangeEmail Tests

        [Fact]
        public void ChangeEmail_WithValidEmail_ShouldUpdateEmail()
        {
            // Arrange
            var user = CreateTestUser();
            var newEmail = new Email("newemail@port.com");

            // Act
            user.ChangeEmail(newEmail);

            // Assert
            user.Email.Should().Be(newEmail);
            user.Email.Value.Should().Be("newemail@port.com");
        }

        [Fact]
        public void ChangeEmail_WithNullEmail_ShouldThrowException()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            var act = () => user.ChangeEmail(null!);

            // Assert
            act.Should().Throw<BusinessRuleValidationException>()
                .WithMessage("Email is required.");
        }

        [Fact]
        public void ChangeEmail_ShouldPreserveOtherProperties()
        {
            // Arrange
            var user = CreateTestUser();
            var originalUsername = user.Username;
            var originalRole = user.Role;
            var originalId = user.Id;
            var newEmail = new Email("updated@port.com");

            // Act
            user.ChangeEmail(newEmail);

            // Assert
            user.Id.Should().Be(originalId);
            user.Username.Should().Be(originalUsername);
            user.Role.Should().Be(originalRole);
        }

        #endregion

        #region ConfirmationToken Tests

        [Fact]
        public void ChangeConfirmationToken_ShouldUpdateToken()
        {
            // Arrange
            var user = CreateTestUser();
            var newToken = "newtoken123";

            // Act
            user.ChangeConfirmationToken(newToken);

            // Assert
            user.ConfirmationToken.Should().Be(newToken);
        }

        [Fact]
        public void GenerateConfirmationToken_ShouldCreateNonEmptyToken()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.GenerateConfirmationToken();

            // Assert
            user.ConfirmationToken.Should().NotBeNullOrEmpty();
            user.ConfirmationToken.Length.Should().Be(32); // GUID without hyphens
        }

        [Fact]
        public void GenerateConfirmationToken_ShouldSetExpiryTo24HoursFromNow()
        {
            // Arrange
            var user = CreateTestUser();
            var beforeGeneration = DateTime.UtcNow;

            // Act
            user.GenerateConfirmationToken();

            // Assert
            var afterGeneration = DateTime.UtcNow;
            user.ConfirmationTokenExpiry.Should().NotBeNull();
            user.ConfirmationTokenExpiry.Should().BeAfter(beforeGeneration.AddHours(23).AddMinutes(59));
            user.ConfirmationTokenExpiry.Should().BeBefore(afterGeneration.AddHours(24).AddMinutes(1));
        }

        [Fact]
        public void GenerateConfirmationToken_ShouldCreateUniqueTokens()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.GenerateConfirmationToken();
            var token1 = user.ConfirmationToken;
            
            user.GenerateConfirmationToken();
            var token2 = user.ConfirmationToken;

            // Assert
            token1.Should().NotBe(token2);
        }

        #endregion

        #region Real-World Scenario Tests

        [Fact]
        public void Scenario_NewAdminRegistration_ShouldCreateActiveUserWithToken()
        {
            // Arrange
            var username = new Username("admin01");
            var role = new Role(RoleType.Admin);
            var email = new Email("admin@portauthority.com");

            // Act
            var user = new ProjArqsi.Domain.UserAggregate.User(username, role, email);
            user.GenerateConfirmationToken();

            // Assert
            user.Should().NotBeNull();
            user.IsActive.Should().BeTrue();
            user.Role.Value.Should().Be(RoleType.Admin);
            user.ConfirmationToken.Should().NotBeEmpty();
            user.ConfirmationTokenExpiry.Should().NotBeNull();
        }

        [Fact]
        public void Scenario_PortOfficerAccountSetup_ShouldCreateInactiveUserPendingConfirmation()
        {
            // Arrange
            var username = new Username("officer_smith");
            var role = new Role(RoleType.PortAuthorityOfficer);
            var email = new Email("smith@portauthority.com");

            // Act
            var user = new ProjArqsi.Domain.UserAggregate.User(username, role, email, isActive: false);
            user.GenerateConfirmationToken();

            // Assert
            user.IsActive.Should().BeFalse();
            user.Role.Value.Should().Be(RoleType.PortAuthorityOfficer);
            user.ConfirmationToken.Should().NotBeEmpty();
            user.ConfirmationTokenExpiry.Should().NotBeNull();
        }

        [Fact]
        public void Scenario_UserActivationAfterEmailConfirmation_ShouldActivateUser()
        {
            // Arrange
            var user = new ProjArqsi.Domain.UserAggregate.User(
                new Username("newoperator"),
                new Role(RoleType.LogisticOperator),
                new Email("operator@logistics.com"),
                isActive: false
            );
            user.GenerateConfirmationToken();

            // Act - Simulate email confirmation process
            user.Activate();
            user.ChangeConfirmationToken(string.Empty); // Clear token after confirmation

            // Assert
            user.IsActive.Should().BeTrue();
            user.ConfirmationToken.Should().BeEmpty();
        }

        [Fact]
        public void Scenario_RolePromotion_ShouldUpgradeUserRole()
        {
            // Arrange
            var user = new ProjArqsi.Domain.UserAggregate.User(
                new Username("junior_operator"),
                new Role(RoleType.ShippingAgentRepresentative),
                new Email("junior@shipping.com")
            );

            // Act - Promote to LogisticOperator
            user.ChangeRole(new Role(RoleType.LogisticOperator));

            // Assert
            user.Role.Value.Should().Be(RoleType.LogisticOperator);
            user.Username.Value.Should().Be("junior_operator"); // Username unchanged
        }

        [Fact]
        public void Scenario_AccountSuspension_ShouldDeactivateUser()
        {
            // Arrange
            var user = CreateTestUser(isActive: true);
            var originalRole = user.Role;

            // Act - Suspend account due to policy violation
            user.Deactivate();

            // Assert
            user.IsActive.Should().BeFalse();
            user.Role.Should().Be(originalRole); // Role preserved for reactivation
        }

        [Fact]
        public void Scenario_EmailChangeRequest_ShouldUpdateEmailAndGenerateNewToken()
        {
            // Arrange
            var user = CreateTestUser();
            var newEmail = new Email("updated.email@port.com");

            // Act
            user.ChangeEmail(newEmail);
            user.GenerateConfirmationToken(); // New verification needed

            // Assert
            user.Email.Value.Should().Be("updated.email@port.com");
            user.ConfirmationToken.Should().NotBeEmpty();
            user.ConfirmationTokenExpiry.Should().NotBeNull();
        }

        [Fact]
        public void Scenario_UsernameChange_ShouldUpdateUsernameWhilePreservingIdentity()
        {
            // Arrange
            var user = CreateTestUser();
            var originalId = user.Id;
            var originalEmail = user.Email;
            var newUsername = new Username("renamed_user");

            // Act
            user.ChangeUsername(newUsername);

            // Assert
            user.Id.Should().Be(originalId); // Identity preserved
            user.Username.Value.Should().Be("renamed_user");
            user.Email.Should().Be(originalEmail); // Email unchanged
        }

        [Fact]
        public void Scenario_MultipleRoleChanges_ShouldReflectLatestRole()
        {
            // Arrange
            var user = new ProjArqsi.Domain.UserAggregate.User(
                new Username("flexible_user"),
                new Role(RoleType.ShippingAgentRepresentative),
                new Email("flexible@port.com")
            );

            // Act - Multiple role changes
            user.ChangeRole(new Role(RoleType.LogisticOperator));
            user.ChangeRole(new Role(RoleType.PortAuthorityOfficer));
            user.ChangeRole(new Role(RoleType.Admin));

            // Assert
            user.Role.Value.Should().Be(RoleType.Admin);
        }

        #endregion

        #region Identity and Equality Tests

        [Fact]
        public void User_WithSameId_ShouldBeEqual()
        {
            // Arrange
            var userId = new UserId(Guid.NewGuid());
            var user1 = CreateTestUser();
            var user2 = CreateTestUser();

            // Use reflection to set same ID (for testing purposes)
            typeof(ProjArqsi.Domain.UserAggregate.User)
                .GetProperty("Id")!
                .SetValue(user1, userId);
            typeof(ProjArqsi.Domain.UserAggregate.User)
                .GetProperty("Id")!
                .SetValue(user2, userId);

            // Act & Assert
            user1.Id.Should().Be(user2.Id);
        }

        [Fact]
        public void User_WithDifferentId_ShouldNotBeEqual()
        {
            // Arrange
            var user1 = CreateTestUser();
            var user2 = CreateTestUser();

            // Act & Assert
            user1.Id.Should().NotBe(user2.Id);
        }

        [Fact]
        public void User_IdShouldBeImmutableAfterMultipleOperations()
        {
            // Arrange
            var user = CreateTestUser();
            var originalId = user.Id;

            // Act - Perform multiple operations
            user.ChangeUsername(new Username("changed1"));
            user.ChangeRole(new Role(RoleType.Admin));
            user.ChangeEmail(new Email("changed@test.com"));
            user.Deactivate();
            user.Activate();
            user.GenerateConfirmationToken();

            // Assert
            user.Id.Should().Be(originalId);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void User_WithMinimumUsernameLength_ShouldBeValid()
        {
            // Arrange
            var username = new Username("abc"); // 3 characters - minimum
            var role = new Role(RoleType.Admin);
            var email = new Email("min@test.com");

            // Act
            var user = new ProjArqsi.Domain.UserAggregate.User(username, role, email);

            // Assert
            user.Username.Value.Should().Be("abc");
        }

        [Fact]
        public void User_WithAllRoleTypes_ShouldBeCreatable()
        {
            // Arrange & Act
            var admin = new ProjArqsi.Domain.UserAggregate.User(
                new Username("admin"), 
                new Role(RoleType.Admin), 
                new Email("admin@test.com")
            );
            var officer = new ProjArqsi.Domain.UserAggregate.User(
                new Username("officer"), 
                new Role(RoleType.PortAuthorityOfficer), 
                new Email("officer@test.com")
            );
            var operator1 = new ProjArqsi.Domain.UserAggregate.User(
                new Username("operator"), 
                new Role(RoleType.LogisticOperator), 
                new Email("operator@test.com")
            );
            var agent = new ProjArqsi.Domain.UserAggregate.User(
                new Username("agent"), 
                new Role(RoleType.ShippingAgentRepresentative), 
                new Email("agent@test.com")
            );

            // Assert
            admin.Role.Value.Should().Be(RoleType.Admin);
            officer.Role.Value.Should().Be(RoleType.PortAuthorityOfficer);
            operator1.Role.Value.Should().Be(RoleType.LogisticOperator);
            agent.Role.Value.Should().Be(RoleType.ShippingAgentRepresentative);
        }

        [Fact]
        public void User_EmptyConfirmationToken_ShouldBeAllowed()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.ChangeConfirmationToken(string.Empty);

            // Assert
            user.ConfirmationToken.Should().BeEmpty();
        }

        [Fact]
        public void User_MultipleTokenGenerations_ShouldUpdateExpiry()
        {
            // Arrange
            var user = CreateTestUser();

            // Act
            user.GenerateConfirmationToken();
            var firstExpiry = user.ConfirmationTokenExpiry;
            
            System.Threading.Thread.Sleep(10); // Small delay
            
            user.GenerateConfirmationToken();
            var secondExpiry = user.ConfirmationTokenExpiry;

            // Assert
            secondExpiry.Should().BeAfter(firstExpiry!.Value);
        }

        #endregion

        #region Helper Methods

        private ProjArqsi.Domain.UserAggregate.User CreateTestUser(bool isActive = true)
        {
            return new ProjArqsi.Domain.UserAggregate.User(
                new Username("testuser"),
                new Role(RoleType.Admin),
                new Email("test@port.com"),
                isActive
            );
        }

        #endregion
    }
}
