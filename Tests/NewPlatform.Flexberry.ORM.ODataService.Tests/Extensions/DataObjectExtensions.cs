namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions
{
    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers;

    using Newtonsoft.Json;

    /// <summary>
    /// Класс, содержащий вспомогательные методы для работы со строками.
    /// </summary>
    public static class DataObjectExtensions
    {
        /// <summary>
        /// Осуществляет преобразование объекта данных <see cref="DataObject"/> в JSON-строку.
        /// </summary>
        /// <param name="dataObject">Преобразуемый объект данных.</param>
        /// <param name="dataObjectView">Представление, по которому будет преобразован объект данных.</param>
        /// <returns>JSON-строка, представляющая объект данных.</returns>
        public static string ToJson(this DataObject dataObject, View dataObjectView, DataObjectEdmModel model)
        {
            return JsonConvert.SerializeObject(new DataObjectDictionary(dataObject, dataObjectView, model, true));
        }
    }
}
