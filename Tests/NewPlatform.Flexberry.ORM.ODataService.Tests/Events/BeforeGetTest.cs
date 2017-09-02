namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Events
{
    using System;
    using System.Net;
    using System.Net.Http;

    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;

    using Xunit;

    /// <summary>
    /// Класс тестов для тестирования логики после операций модификации данных OData-сервисом (вставка, обновление, удаление).
    /// </summary>
    
    public class BeforeGetTest : BaseODataServiceIntegratedTest
    {
        private LoadingCustomizationStruct lcs { get; set; }

        /// <summary>
        /// Метод вызываемый перед загрузкой объектов.
        /// </summary>
        /// <param name="lcs"></param>
        /// <returns></returns>
        public bool BeforeGet(ref LoadingCustomizationStruct lcs)
        {
            this.lcs = lcs;
            return true;
        }

        /// <summary>
        /// Осуществляет проверку того, что при POST запросах происходит вставка объекта,
        /// зависимые объекты (мастера, детейлы) обрабатываются в зависимости от наличия в БД - вставляются или обновляются.
        /// </summary>
        [Fact]
        public void TestBeforeGet()
        {

            ActODataService(args =>
            {
                args.Token.Events.CallbackBeforeGet = BeforeGet;

                DateTime date = new DateTimeOffset(DateTime.Now).UtcDateTime;
                string prevDate = $"{date.AddDays(-1).ToString("yyyy-MM-ddTHH:mm:ss")}%2B05:00";
                string nextDate = $"{date.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ss")}%2B05:00";
                КлассСМножествомТипов класс = new КлассСМножествомТипов() { PropertyEnum = Цифра.Семь, PropertyDateTime = date };
                Медведь медв = new Медведь { Вес = 48, Пол = tПол.Мужской };
                Медведь медв2 = new Медведь { Вес = 148, Пол = tПол.Мужской };
                Лес лес1 = new Лес { Название = "Бор" };
                Лес лес2 = new Лес { Название = "Березовая роща" };
                медв.ЛесОбитания = лес1;
                var берлога1 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                var берлога2 = new Берлога { Наименование = "Для плохого настроения", ЛесРасположения = лес2 };
                var берлога3 = new Берлога { Наименование = "Для хорошего настроения", ЛесРасположения = лес1 };
                медв.Берлога.Add(берлога1);
                медв.Берлога.Add(берлога2);
                медв2.Берлога.Add(берлога3);
                var objs = new DataObject[] { класс, медв, медв2, берлога2, берлога1, берлога3, лес1, лес2 };
                args.DataService.UpdateObjects(ref objs);
                this.lcs = null;
                var requestUrl = "http://localhost/odata/Медведьs";

                // Обращаемся к OData-сервису и обрабатываем ответ.
                using (HttpResponseMessage response = args.HttpClient.GetAsync(requestUrl).Result)
                {
                    // Убедимся, что запрос завершился успешно.
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.NotNull(this.lcs);
                }
            });
        }
    }
}
