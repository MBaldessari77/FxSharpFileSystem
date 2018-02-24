using SharpFileSystem.Collections;
using SharpFileSystem.FileSystems;
using Xunit;

namespace SharpFileSystem.Tests.FileSystems
{
	public class EntityMoverRegistrationTest
	{
		public EntityMoverRegistrationTest()
		{
			_registration = new TypeCombinationDictionary<IEntityMover>();
			_registration.AddLast(typeof(PhysicalFileSystem), typeof(PhysicalFileSystem), _physicalEntityMover);
			_registration.AddLast(typeof(IFileSystem), typeof(IFileSystem), _standardEntityMover);
		}

		readonly TypeCombinationDictionary<IEntityMover> _registration;
		readonly IEntityMover _physicalEntityMover = new PhysicalEntityMover();
		readonly IEntityMover _standardEntityMover = new StandardEntityMover();

		[Fact]
		public void When_MovingFromGenericToGenericFileSystem_Expect_StandardEntityMover()
		{
			Assert.Equal(
				_standardEntityMover,
				_registration.GetSupportedRegistration(typeof(IFileSystem), typeof(IFileSystem)).Value
			);
		}

		[Fact]
		public void When_MovingFromOtherToPhysicalFileSystem_Expect_StandardEntityMover()
		{
			Assert.Equal(
				_standardEntityMover,
				_registration.GetSupportedRegistration(typeof(IFileSystem), typeof(PhysicalFileSystem)).Value
			);
		}

		[Fact]
		public void When_MovingFromPhysicalToGenericFileSystem_Expect_StandardEntityMover()
		{
			Assert.Equal(
				_standardEntityMover,
				_registration.GetSupportedRegistration(typeof(PhysicalFileSystem), typeof(IFileSystem)).Value
			);
		}

		[Fact]
		public void When_MovingFromPhysicalToPhysicalFileSystem_Expect_PhysicalEntityMover()
		{
			Assert.Equal(
				_physicalEntityMover,
				_registration.GetSupportedRegistration(typeof(PhysicalFileSystem), typeof(PhysicalFileSystem)).Value
			);
		}
	}
}
