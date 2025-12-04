using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjArqsi.Domain.UserAggregate;
using ProjArqsi.Domain.UserAggregate.ValueObjects;

namespace ProjArqsi.Infrastructure
{
	public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
	{
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.HasKey(u => u.Id);
			builder.Property(u => u.Id)
				.HasConversion(
					id => id.AsGuid(),
					value => new UserId(value)
				);

			builder.Property(u => u.Email)
				.HasConversion(
					email => email.Value,
					value => new Email(value)
				)
				.HasColumnName("Email")
				.IsRequired();

			builder.Property(u => u.Username)
				.HasConversion(
					username => username.Value,
					value => new Username(value)
				)
				.HasColumnName("Username")
				.IsRequired();

			builder.Property(u => u.Role)
				.HasConversion(
					role => role.Value,
					value => new Role(value)
				)
				.HasColumnName("Role")
				.IsRequired();

			builder.Property(u => u.IsActive).IsRequired();
			builder.Property(u => u.ConfirmationToken).HasMaxLength(100);
		}
	}
}
