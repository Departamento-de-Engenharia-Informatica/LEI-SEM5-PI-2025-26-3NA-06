using FluentAssertions;
using ProjArqsi.Domain.UserAggregate.ValueObjects;
using Xunit;

namespace ProjArqsi.Tests.ValueObjectTests.User
{
    public class RoleTests
    {
        #region Valid Role Tests

        [Fact]
        public void Constructor_WithAdminRole_ShouldCreateRole()
        {
            // Act
            var role = new Role(RoleType.Admin);

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.Admin);
        }

        [Fact]
        public void Constructor_WithPortAuthorityOfficerRole_ShouldCreateRole()
        {
            // Act
            var role = new Role(RoleType.PortAuthorityOfficer);

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.PortAuthorityOfficer);
        }

        [Fact]
        public void Constructor_WithLogisticOperatorRole_ShouldCreateRole()
        {
            // Act
            var role = new Role(RoleType.LogisticOperator);

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.LogisticOperator);
        }

        [Fact]
        public void Constructor_WithShippingAgentRepresentativeRole_ShouldCreateRole()
        {
            // Act
            var role = new Role(RoleType.ShippingAgentRepresentative);

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.ShippingAgentRepresentative);
        }

        [Theory]
        [InlineData(RoleType.Admin)]
        [InlineData(RoleType.PortAuthorityOfficer)]
        [InlineData(RoleType.LogisticOperator)]
        [InlineData(RoleType.ShippingAgentRepresentative)]
        public void Constructor_WithAllValidRoles_ShouldCreateRole(RoleType roleType)
        {
            // Act
            var role = new Role(roleType);

            // Assert
            role.Value.Should().Be(roleType);
        }

        #endregion

        #region Static Factory Tests

        [Fact]
        public void Roles_Admin_ShouldReturnAdminRole()
        {
            // Act
            var role = Roles.Admin;

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.Admin);
        }

        [Fact]
        public void Roles_PortAuthorityOfficer_ShouldReturnPortAuthorityOfficerRole()
        {
            // Act
            var role = Roles.PortAuthorityOfficer;

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.PortAuthorityOfficer);
        }

        [Fact]
        public void Roles_LogisticOperator_ShouldReturnLogisticOperatorRole()
        {
            // Act
            var role = Roles.LogisticOperator;

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.LogisticOperator);
        }

        [Fact]
        public void Roles_ShippingAgentRepresentative_ShouldReturnShippingAgentRepresentativeRole()
        {
            // Act
            var role = Roles.ShippingAgentRepresentative;

            // Assert
            role.Should().NotBeNull();
            role.Value.Should().Be(RoleType.ShippingAgentRepresentative);
        }

        [Fact]
        public void Roles_Admin_CalledMultipleTimes_ShouldReturnEqualObjects()
        {
            // Act
            var role1 = Roles.Admin;
            var role2 = Roles.Admin;

            // Assert
            role1.Should().Be(role2);
        }

        [Fact]
        public void Roles_PortAuthorityOfficer_CalledMultipleTimes_ShouldReturnEqualObjects()
        {
            // Act
            var role1 = Roles.PortAuthorityOfficer;
            var role2 = Roles.PortAuthorityOfficer;

            // Assert
            role1.Should().Be(role2);
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_WithAdminRole_ShouldReturnAdmin()
        {
            // Arrange
            var role = new Role(RoleType.Admin);

            // Act
            var result = role.ToString();

            // Assert
            result.Should().Be("Admin");
        }

        [Fact]
        public void ToString_WithPortAuthorityOfficerRole_ShouldReturnPortAuthorityOfficer()
        {
            // Arrange
            var role = new Role(RoleType.PortAuthorityOfficer);

            // Act
            var result = role.ToString();

            // Assert
            result.Should().Be("PortAuthorityOfficer");
        }

        [Fact]
        public void ToString_WithLogisticOperatorRole_ShouldReturnLogisticOperator()
        {
            // Arrange
            var role = new Role(RoleType.LogisticOperator);

            // Act
            var result = role.ToString();

            // Assert
            result.Should().Be("LogisticOperator");
        }

        [Fact]
        public void ToString_WithShippingAgentRepresentativeRole_ShouldReturnShippingAgentRepresentative()
        {
            // Arrange
            var role = new Role(RoleType.ShippingAgentRepresentative);

            // Act
            var result = role.ToString();

            // Assert
            result.Should().Be("ShippingAgentRepresentative");
        }

        [Theory]
        [InlineData(RoleType.Admin, "Admin")]
        [InlineData(RoleType.PortAuthorityOfficer, "PortAuthorityOfficer")]
        [InlineData(RoleType.LogisticOperator, "LogisticOperator")]
        [InlineData(RoleType.ShippingAgentRepresentative, "ShippingAgentRepresentative")]
        public void ToString_WithAllRoles_ShouldReturnCorrectString(RoleType roleType, string expected)
        {
            // Arrange
            var role = new Role(roleType);

            // Act
            var result = role.ToString();

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Equality Tests

        [Fact]
        public void Equals_WithSameAdminRole_ShouldReturnTrue()
        {
            // Arrange
            var role1 = new Role(RoleType.Admin);
            var role2 = new Role(RoleType.Admin);

            // Act
            var result = role1.Equals(role2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithSamePortAuthorityOfficerRole_ShouldReturnTrue()
        {
            // Arrange
            var role1 = new Role(RoleType.PortAuthorityOfficer);
            var role2 = new Role(RoleType.PortAuthorityOfficer);

            // Act
            var result = role1.Equals(role2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void Equals_WithDifferentRoles_ShouldReturnFalse()
        {
            // Arrange
            var role1 = new Role(RoleType.Admin);
            var role2 = new Role(RoleType.LogisticOperator);

            // Act
            var result = role1.Equals(role2);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_UsingStaticFactories_ShouldWork()
        {
            // Arrange
            var role1 = Roles.Admin;
            var role2 = new Role(RoleType.Admin);

            // Act
            var result = role1.Equals(role2);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetHashCode_WithSameRole_ShouldReturnSameHashCode()
        {
            // Arrange
            var role1 = new Role(RoleType.Admin);
            var role2 = new Role(RoleType.Admin);

            // Act
            var hash1 = role1.GetHashCode();
            var hash2 = role2.GetHashCode();

            // Assert
            hash1.Should().Be(hash2);
        }

        [Fact]
        public void GetHashCode_WithDifferentRoles_ShouldReturnDifferentHashCode()
        {
            // Arrange
            var role1 = new Role(RoleType.Admin);
            var role2 = new Role(RoleType.LogisticOperator);

            // Act
            var hash1 = role1.GetHashCode();
            var hash2 = role2.GetHashCode();

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion

        #region Enum Tests

        [Fact]
        public void RoleType_ShouldHaveAdminAsZero()
        {
            // Assert
            ((int)RoleType.Admin).Should().Be(0);
        }

        [Fact]
        public void RoleType_ShouldHavePortAuthorityOfficerAsOne()
        {
            // Assert
            ((int)RoleType.PortAuthorityOfficer).Should().Be(1);
        }

        [Fact]
        public void RoleType_ShouldHaveLogisticOperatorAsTwo()
        {
            // Assert
            ((int)RoleType.LogisticOperator).Should().Be(2);
        }

        [Fact]
        public void RoleType_ShouldHaveShippingAgentRepresentativeAsThree()
        {
            // Assert
            ((int)RoleType.ShippingAgentRepresentative).Should().Be(3);
        }

        [Fact]
        public void RoleType_ShouldHaveExactlyFourValues()
        {
            // Act
            var values = Enum.GetValues(typeof(RoleType));

            // Assert
            values.Length.Should().Be(4);
        }

        [Fact]
        public void RoleType_AllValues_ShouldBeUsableInConstructor()
        {
            // Arrange
            var allValues = Enum.GetValues(typeof(RoleType)).Cast<RoleType>();

            // Act & Assert
            foreach (var value in allValues)
            {
                var role = new Role(value);
                role.Value.Should().Be(value);
            }
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void Constructor_WithInvalidEnumValue_ShouldStillCreateObject()
        {
            // Arrange - Cast an invalid int to enum
            var invalidEnum = (RoleType)999;

            // Act
            var role = new Role(invalidEnum);

            // Assert
            role.Value.Should().Be(invalidEnum);
        }

        [Fact]
        public void Value_AfterConstruction_ShouldBeImmutable()
        {
            // Arrange
            var role = new Role(RoleType.Admin);

            // Act
            var retrievedValue = role.Value;

            // Assert
            retrievedValue.Should().Be(RoleType.Admin);
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var role = new Role(RoleType.Admin);

            // Act
            var result = role.Equals(null);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Equals_WithArbitraryObject_ShouldReturnFalse()
        {
            // Arrange
            var role = new Role(RoleType.Admin);
            var arbitraryObject = new { Role = "Admin" };

            // Act
            var result = role.Equals(arbitraryObject);

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region Static Factory Consistency Tests

        [Fact]
        public void StaticFactories_ShouldReturnCorrectRoleTypes()
        {
            // Act & Assert
            Roles.Admin.Value.Should().Be(RoleType.Admin);
            Roles.PortAuthorityOfficer.Value.Should().Be(RoleType.PortAuthorityOfficer);
            Roles.LogisticOperator.Value.Should().Be(RoleType.LogisticOperator);
            Roles.ShippingAgentRepresentative.Value.Should().Be(RoleType.ShippingAgentRepresentative);
        }

        [Fact]
        public void StaticFactories_ShouldBeEquivalentToConstructor()
        {
            // Act & Assert
            Roles.Admin.Should().Be(new Role(RoleType.Admin));
            Roles.PortAuthorityOfficer.Should().Be(new Role(RoleType.PortAuthorityOfficer));
            Roles.LogisticOperator.Should().Be(new Role(RoleType.LogisticOperator));
            Roles.ShippingAgentRepresentative.Should().Be(new Role(RoleType.ShippingAgentRepresentative));
        }

        #endregion

        #region Real-World Scenarios

        [Fact]
        public void Role_ForSystemAdministrator_ShouldBeAdmin()
        {
            // Act
            var role = Roles.Admin;

            // Assert
            role.Value.Should().Be(RoleType.Admin);
            role.ToString().Should().Be("Admin");
        }

        [Fact]
        public void Role_ForPortOfficer_ShouldBePortAuthorityOfficer()
        {
            // Act
            var role = Roles.PortAuthorityOfficer;

            // Assert
            role.Value.Should().Be(RoleType.PortAuthorityOfficer);
            role.ToString().Should().Be("PortAuthorityOfficer");
        }

        [Fact]
        public void Role_ForWarehouseManager_ShouldBeLogisticOperator()
        {
            // Act
            var role = Roles.LogisticOperator;

            // Assert
            role.Value.Should().Be(RoleType.LogisticOperator);
            role.ToString().Should().Be("LogisticOperator");
        }

        [Fact]
        public void Role_ForShippingCompanyAgent_ShouldBeShippingAgentRepresentative()
        {
            // Act
            var role = Roles.ShippingAgentRepresentative;

            // Assert
            role.Value.Should().Be(RoleType.ShippingAgentRepresentative);
            role.ToString().Should().Be("ShippingAgentRepresentative");
        }

        #endregion
    }
}
