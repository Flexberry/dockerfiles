namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Model
{
    using System;
    using System.Collections.Generic;

    using NewPlatform.Flexberry.ORM.ODataService.Model;

    using Xunit;

    /// <summary>
    /// Тесты класса <see cref="DynamicView" />.
    /// </summary>
    public class DynamicViewTests
    {
        /// <summary>
        /// Тест проверяет, что метод возвращает все свойства класса.
        /// У класса отсутствуют представления.
        /// </summary>
        [Fact]
        public void GetPropertiesNoViewsTest()
        {
            // Arrange.
            Type type = typeof(Master);

            // Act.
            List<string> props = DynamicView.GetProperties(type);

            // Assert.
            Assert.Equal(1, props.Count);
        }

        /// <summary>
        /// Тест проверяет, что метод возвращает все свойства класса.
        /// У класса нет E-представления.
        /// </summary>
        [Fact]
        public void GetPropertiesNoEditViewTest()
        {
            // Arrange.
            Type type = typeof(Driver);

            // Act.
            List<string> props = DynamicView.GetProperties(type);

            // Assert.
            Assert.Equal(4, props.Count);
        }

        /// <summary>
        /// Тест проверяет, что метод возвращает свойства, кроме нехранимого, т.к. его нет в дефолтном представлении.
        /// </summary>
        [Fact]
        public void GetPropertiesNotStoredPropertyTest()
        {
            // Arrange.
            Type type = typeof(КлассСМножествомТипов);

            // Act.
            List<string> props = DynamicView.GetProperties(type);

            // Assert.
            Assert.DoesNotContain(props, p => p == nameof(КлассСМножествомТипов.NotStoredProperty));
        }

        /// <summary>
        /// Тест проверяет, что метод возвращает все свойства класса с наследованием.
        /// </summary>
        [Fact]
        public void GetPropertiesHierarchyObjectTest()
        {
            // Arrange.
            Type type = typeof(Наследник);

            // Act.
            List<string> props = DynamicView.GetProperties(type);

            // Assert.
            Assert.Equal(6, props.Count);
        }
    }
}
