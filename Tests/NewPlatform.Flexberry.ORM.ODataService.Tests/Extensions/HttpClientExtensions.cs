namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions
{
    using System;
    using System.Net.Http;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    using ICSSoft.STORMNET;

    using NewPlatform.Flexberry.ORM.ODataService.Model;

    /// <summary>
    /// Класс, содержащий вспомогательные методы для работы с <see cref="HttpClient"/>.
    /// </summary>
    public static class HttpClientExtensions
    {
        public static Task<HttpResponseMessage> GetAsync(this HttpClient httpClient, DataObjectEdmModel model, Type dataObjectType, string lockUserName = null)
        {
            IPrincipal currentPrincipal = null;
            try
            {
                if (lockUserName != null)
                {
                    currentPrincipal = Thread.CurrentPrincipal;
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(lockUserName), null);

                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Flexberry-Sync", "Lock");
                }

                return httpClient.GetAsync(model.GetEdmEntitySet(dataObjectType).Name);
            }
            finally
            {
                if (lockUserName != null)
                {
                    Thread.CurrentPrincipal = currentPrincipal;

                    httpClient.DefaultRequestHeaders.Remove("Flexberry-Sync");
                }
            }
        }

        /// <summary>
        /// Осуществляет POST-запрос с передачей JSON-строки по заданному пути.
        /// </summary>
        /// <param name="httpClient">Клиент, через которые будут осуществляться запрос.</param>
        /// <param name="url">URL-запроса.</param>
        /// <param name="jsonData">JSON-строка с данными, которая должна быть отправлена в запросе.</param>
        /// <returns>Ответ на запрос.</returns>
        public static Task<HttpResponseMessage> PostAsJsonStringAsync(this HttpClient httpClient, string url, string jsonData)
        {
            return SendRequestAsJsonString(httpClient, url, jsonData, "POST");
        }

        /// <summary>
        /// Осуществляет PUT-запрос с передачей JSON-строки по заданному пути.
        /// </summary>
        /// <param name="httpClient">Клиент, через которые будут осуществляться запрос.</param>
        /// <param name="url">URL-запроса.</param>
        /// <param name="jsonData">JSON-строка с данными, которая должна быть отправлена в запросе.</param>
        /// <returns>Ответ на запрос.</returns>
        public static Task<HttpResponseMessage> PutAsJsonStringAsync(this HttpClient httpClient, string url, string jsonData)
        {
            return SendRequestAsJsonString(httpClient, url, jsonData, "PUT");
        }

        /// <summary>
        /// Осуществляет PATCH-запрос с передачей JSON-строки по заданному пути.
        /// </summary>
        /// <param name="httpClient">Клиент, через которые будут осуществляться запрос.</param>
        /// <param name="url">URL-запроса.</param>
        /// <param name="jsonData">JSON-строка с данными, которая должна быть отправлена в запросе.</param>
        /// <returns>Ответ на запрос.</returns>
        public static Task<HttpResponseMessage> PatchAsJsonStringAsync(this HttpClient httpClient, string url, string jsonData)
        {
            return SendRequestAsJsonString(httpClient, url, jsonData, "PATCH");
        }

        public static Task<HttpResponseMessage> PatchAsJsonStringAsync(this HttpClient httpClient, DataObjectEdmModel model, DataObject dataObject, View view, string unlockUserName = null)
        {
            var pk = ((ICSSoft.STORMNET.KeyGen.KeyGuid)dataObject.__PrimaryKey).Guid.ToString();
            var url = $"{model.GetEdmEntitySet(typeof(Лес)).Name}({pk})";
            var jsonData = dataObject.ToJson(Лес.Views.ЛесE, model);

            IPrincipal currentPrincipal = null;
            try
            {
                if (unlockUserName != null)
                {
                    currentPrincipal = Thread.CurrentPrincipal;
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(unlockUserName), null);

                    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Flexberry-Sync", "Unlock");
                }

                return SendRequestAsJsonString(httpClient, url, jsonData, "PATCH");
            }
            finally
            {
                if (unlockUserName != null)
                {
                    Thread.CurrentPrincipal = currentPrincipal;

                    httpClient.DefaultRequestHeaders.Remove("Flexberry-Sync");
                }
            }
        }

        /// <summary>
        /// Осуществляет HTTP-запрос указанного типа с передачей JSON-строки по заданному пути.
        /// </summary>
        /// <param name="httpClient">Клиент, через которые будут осуществляться запрос.</param>
        /// <param name="url">URL-запроса.</param>
        /// <param name="jsonData">JSON-строка с данными, которая должна быть отправлена в запросе.</param>
        /// <param name="httpMethodName">Тип HTTP-запроса ("PUT", "POST", "PATCH". и т.д.).</param>
        /// <returns>Ответ на запрос.</returns>
        private static Task<HttpResponseMessage> SendRequestAsJsonString(HttpClient httpClient, string url, string jsonData, string httpMethodName)
        {
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod(httpMethodName), url) { Content = content };

            return httpClient.SendAsync(request);
        }
    }
}
