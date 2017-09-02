namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Model
{
    using ICSSoft.STORMNET;

    using Xunit;

    using NewPlatform.Flexberry.ORM.ODataService.Model;

    
    public class DefaultDataObjectModelBuilderTest
    {
        private class H1 : DataObject
        {
        }

        private class H2 : H1
        {
        }

        [Fact]
        public void TestDataObjectIsNotRegisteredInEmptyModel()
        {
            var model = new DataObjectEdmModel(new DataObjectEdmMetadata());

            Assert.False(model.IsDataObjectRegistered(typeof(DataObject)));
        }

        [Fact]
        public void TestRegisteringHierarchy()
        {
            var builder = new DefaultDataObjectEdmModelBuilder(new[] { GetType().Assembly });

            DataObjectEdmModel model = builder.Build();

            Assert.True(model.IsDataObjectRegistered(typeof(DataObject)));
            Assert.True(model.IsDataObjectRegistered(typeof(H1)));
            Assert.True(model.IsDataObjectRegistered(typeof(H2)));
        }
    }
}
