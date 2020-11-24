namespace NewPlatform.Flexberry.ORM.ODataService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using Microsoft.AspNet.OData;
    using Microsoft.OData.Edm;
    using NewPlatform.Flexberry.ORM.ODataService.Batch;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Files;
    using NewPlatform.Flexberry.ORM.ODataService.Files.Providers;
    using NewPlatform.Flexberry.ORM.ODataService.Formatter;
    using Newtonsoft.Json;
    using File = ICSSoft.STORMNET.FileType.File;
    using KeySegment = Microsoft.OData.UriParser.KeySegment;

#if NETFRAMEWORK
    using System.Net.Http.Formatting;
    using System.Web.Http;
    using System.Web.Http.Results;
    using System.Web.Http.Validation;
    using NewPlatform.Flexberry.ORM.ODataService.Events;
    using NewPlatform.Flexberry.ORM.ODataService.Handlers;
    using NewPlatform.Flexberry.ORM.ODataService.WebApi.Controllers;
#endif
#if NETSTANDARD
    using Microsoft.AspNet.OData.Formatter;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Formatters;
    using Microsoft.Extensions.Primitives;
    using NewPlatform.Flexberry.ORM.ODataService.Middleware;
#endif

    /// <summary>
    /// Определяет класс контроллера OData, который поддерживает запись и чтение данных с использованием OData формата.
    /// </summary>
    public partial class DataObjectController
    {
#if NETFRAMEWORK
        /// <summary>
        /// Метаданные файлов, загруженных во временную папку <see cref="FileController.UploadsDirectoryPath"/>,
        /// и привязанных к свойствам обрабатываемых объектов данных.
        /// Файлы будут удалены из файловой системы в случае успешного сохранения объектов данных.
        /// </summary>
#elif NETSTANDARD
        /// <summary>
        /// Метаданные файлов, временно загруженных в каталог файлового хранилища и привязанных к свойствам обрабатываемых объектов данных.
        /// Файлы будут удалены из файловой системы <see cref="IDataObjectFileAccessor.RemoveFileUploadDirectories"/>
        /// в случае успешного сохранения объектов данных.
        /// </summary>
#endif
        private List<FileDescription> _removingFileDescriptions = new List<FileDescription>();

        /// <summary>
        /// Создание сущности и всех связанных. При существовании в БД произойдёт обновление.
        /// </summary>
        /// <param name="edmEntity"> Создаваемая сущность. </param>
        /// <returns> Созданная сущность. </returns>
#if NETFRAMEWORK
        public HttpResponseMessage Post([FromBody] EdmEntityObject edmEntity)
#elif NETSTANDARD
        public IActionResult Post([FromBody] EdmEntityObject edmEntity)
#endif
        {
            try
            {
                if (edmEntity == null)
                {
                    edmEntity = ReplaceOdataBindNull();
                }

                DataObject obj = UpdateObject(edmEntity, null);
                ExecuteCallbackAfterCreate(obj);

                edmEntity = GetEdmObject(_model.GetEdmEntityType(type), obj, 1, null, null);
                var responseForPreferMinimal = TestPreferMinimal();
                if (responseForPreferMinimal != null)
                {
                    return responseForPreferMinimal;
                }

#if NETFRAMEWORK
                var result = Request.CreateResponse(HttpStatusCode.Created, edmEntity);
                if (Request.Headers.Contains("Prefer"))
                {
                    result.Headers.Add("Preference-Applied", "return=representation");
                }
#elif NETSTANDARD
                var result = new ObjectResult(edmEntity) { StatusCode = StatusCodes.Status201Created };
                if (Request.Headers.ContainsKey("Prefer"))
                {
                    Response.Headers.Add("Preference-Applied", "return=representation");
                }
#endif

                return result;
            }
            catch (Exception ex)
            {
#if NETFRAMEWORK
                return InternalServerErrorMessage(ex);
#elif NETSTANDARD
                throw CustomException(ex);
#endif
            }
        }

        /// <summary>
        /// Обновление сущности (свойства могут быть заданы частично, т.е. указывать можно значения только измененных свойств).
        /// Если сущности с заданным ключом нет в БД происходит Upsert (в соответствии со стандартом).
        /// </summary>
        /// <param name="key"> Ключ обновляемой сущности. </param>
        /// <param name="edmEntity">Обновляемая сущность. </param>
        /// <returns>Обновлённая сущность.</returns>
#if NETFRAMEWORK
        public HttpResponseMessage Patch([FromODataUri] Guid key, [FromBody] EdmEntityObject edmEntity)
#elif NETSTANDARD
        public IActionResult Patch([FromODataUri] Guid key, [FromBody] EdmEntityObject edmEntity)
#endif
        {
            try
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }

                if (edmEntity == null)
                {
                    edmEntity = ReplaceOdataBindNull();
                }

                IEdmEntityType entityType = (IEdmEntityType)edmEntity.ActualEdmType;

#if NETFRAMEWORK
                var dictionary = Request.Properties.ContainsKey(ExtendedODataEntityDeserializer.Dictionary) ?
                    (Dictionary<string, object>)Request.Properties[ExtendedODataEntityDeserializer.Dictionary] :
                    new Dictionary<string, object>();
#elif NETSTANDARD
                var dictionary = Request.HttpContext.Items.ContainsKey(ExtendedODataEntityDeserializer.Dictionary) ?
                    (Dictionary<string, object>)Request.HttpContext.Items[ExtendedODataEntityDeserializer.Dictionary] :
                    new Dictionary<string, object>();
#endif

                foreach (var prop in entityType.Properties())
                {
                    if (!dictionary.ContainsKey(prop.Name) && edmEntity.GetChangedPropertyNames().Contains(prop.Name) && prop is EdmNavigationProperty)
                    {
                        const string msg = "Error processing request stream. Deep updates are not supported in PUT or PATCH operations.";
#if NETFRAMEWORK
                        return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
#elif NETSTANDARD
                        return BadRequest(msg);
#endif
                    }

                    if (dictionary.ContainsKey(prop.Name) && dictionary[prop.Name] == null &&
                        (!prop.Type.IsNullable || prop.Type.IsCollection()))
                    {
                        string msg = $"The property {prop.Name} can not be null.";
#if NETFRAMEWORK
                        return Request.CreateResponse(HttpStatusCode.BadRequest, msg);
#elif NETSTANDARD
                        return BadRequest(msg);
#endif
                    }
                }

                DataObject obj = UpdateObject(edmEntity, key);
                ExecuteCallbackAfterUpdate(obj);

                var responseForPreferMinimal = TestPreferMinimal();
                if (responseForPreferMinimal != null)
                {
                    return responseForPreferMinimal;
                }

#if NETFRAMEWORK
                if (!Request.Headers.Contains("Prefer"))
                {
                    return Request.CreateResponse(HttpStatusCode.NoContent);
                }
#elif NETSTANDARD
                if (!Request.Headers.ContainsKey("Prefer"))
                {
                    return NoContent();
                }
#endif

                edmEntity = GetEdmObject(_model.GetEdmEntityType(type), obj, 1, null, null);
#if NETFRAMEWORK
                var result = Request.CreateResponse(HttpStatusCode.OK, edmEntity);
                result.Headers.Add("Preference-Applied", "return=representation");
#elif NETSTANDARD
                var result = Ok(edmEntity);
                Response.Headers.Add("Preference-Applied", "return=representation");
#endif
                return result;
            }
            catch (Exception ex)
            {
#if NETFRAMEWORK
                return InternalServerErrorMessage(ex);
#elif NETSTANDARD
                throw CustomException(ex);
#endif
            }
        }

        /// <summary>
        /// Осуществляет удаление сущности.
        /// </summary>
        /// <returns>
        /// Результат выполнения запроса типа <see cref="StatusCodeResult"/>, соответствующий статусу <see cref="HttpStatusCode.NoContent"/>.
        /// </returns>
#if NETFRAMEWORK
        public HttpResponseMessage DeleteString()
#elif NETSTANDARD
        public NoContentResult DeleteString()
#endif
        {
            var keySegment = ODataPath.Segments[1] as KeySegment;
            string key = keySegment.Keys.First().Value.ToString().Trim().Replace("'", string.Empty);
            return DeleteEntity(key);
        }

        /// <summary>
        /// Осуществляет удаление сущности.
        /// </summary>
        /// <returns>
        /// Результат выполнения запроса типа <see cref="StatusCodeResult"/>, соответствующий статусу <see cref="HttpStatusCode.NoContent"/>.
        /// </returns>
#if NETFRAMEWORK
        public HttpResponseMessage DeleteGuid()
#elif NETSTANDARD
        public NoContentResult DeleteGuid()
#endif
        {
            var keySegment = ODataPath.Segments[1] as KeySegment;
            Guid key = new Guid(keySegment.Keys.First().Value.ToString());
            return DeleteEntity(key);
        }

#if NETFRAMEWORK
        private HttpResponseMessage DeleteEntity(object key)
#elif NETSTANDARD
        private NoContentResult DeleteEntity(object key)
#endif
        {
            try
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                Init();

                var obj = DataObjectCache.CreateDataObject(type, key);

                // Удаляем объект с заданным ключем.
                // Детейлы удалятся вместе с агрегатором автоматически.
                // Если удаляемый объект является мастером для какого-либо объекта, то
                // спецификация предполагает, что зависимые объекты будут каскадно удалены либо ссылки в них заменены
                // на null/значению по умолчанию, если это задано в модели через ReferentialConstraints.
                // Но если это задано в модели, то соответвующие объекты данных реализуют интерфейсы
                // IReferencesCascadeDelete/IReferencesNullDelete и требуемые действия будут выполнены автоматически.
                // В данный момент ReferentialConstraints не создаются в модели.
                obj.SetStatus(ObjectStatus.Deleted);

                // Раз объект данных удаляется, то и все ассоциированные с ним файлы должны быть удалены.
                // Запоминаем метаданные всех ассоциированных файлов, кроме файлов соответствующих файловым свойствам типа File
                // (файлы соответствующие свойствам типа File хранятся в БД, и из файловой системы просто нечего удалять).
                // TODO: подумать как быть с детейлами, детейлами детейлов, и т д.
                var descriptions = _dataObjectFileAccessor.GetDataObjectFileDescriptions(_dataService, obj, new List<Type> { typeof(File) });
                _removingFileDescriptions.AddRange(descriptions);

                List<DataObject> objs = new List<DataObject>();

                if (ExecuteCallbackBeforeDelete(obj))
                {
                    string agregatorPropertyName = Information.GetAgregatePropertyName(type);
                    if (!string.IsNullOrEmpty(agregatorPropertyName))
                    {
                        DataObject agregator = (DataObject)Information.GetPropValueByName(obj, agregatorPropertyName);

                        if (agregator != null)
                        {
                            objs.Add(agregator);
                        }
                    }

                    objs.Add(obj);

                    if (IsBatchChangeSetRequest)
                    {
#if NETFRAMEWORK
                        List<DataObject> dataObjectsToUpdate = (List<DataObject>)Request.Properties[DataObjectODataBatchHandler.DataObjectsToUpdatePropertyKey];
#elif NETSTANDARD
                        List<DataObject> dataObjectsToUpdate = (List<DataObject>)HttpContext.Items[DataObjectODataBatchHandler.DataObjectsToUpdatePropertyKey];
#endif
                        dataObjectsToUpdate.AddRange(objs);
                    }
                    else
                    {
                        DataObject[] dataObjects = objs.ToArray();
                        _dataService.UpdateObjects(ref dataObjects);
                    }
                }

                // При успешном удалении вычищаем из файловой системы, файлы подлежащие удалению.
                _dataObjectFileAccessor.RemoveFileUploadDirectories(_removingFileDescriptions);
                ExecuteCallbackAfterDelete(obj);

#if NETFRAMEWORK
                return Request.CreateResponse(HttpStatusCode.NoContent);
#elif NETSTANDARD
                return NoContent();
#endif
            }
            catch (Exception ex)
            {
                _removingFileDescriptions.Clear();
#if NETFRAMEWORK
                return InternalServerErrorMessage(ex);
#elif NETSTANDARD
                throw CustomException(ex);
#endif
            }
        }

#if NETFRAMEWORK
        /// <summary>
        /// Создаётся http-ответ с кодом 500 по-умолчанию, на возникшую в сервисе ошибку.
        /// Для изменения возвращаемого кода необходимо реализовать обработчик CallbackAfterInternalServerError.
        /// </summary>
        /// <param name="ex">Ошибка сервиса.</param>
        /// <returns>Http-ответ.</returns>
        private HttpResponseMessage InternalServerErrorMessage(Exception ex)
        {
            return InternalServerErrorMessage(ex, _events, Request);
        }

        /// <summary>
        /// Создаётся http-ответ с кодом 500 по-умолчанию, на возникшую в сервисе ошибку.
        /// Для изменения возвращаемого кода необходимо реализовать обработчик CallbackAfterInternalServerError.
        /// </summary>
        /// <param name="ex">Ошибка сервиса.</param>
        /// <param name="events">The container with registered events.</param>
        /// <param name="request">Original HTTP request message for create a response.</param>
        /// <returns>Http-ответ.</returns>
        public static HttpResponseMessage InternalServerErrorMessage(Exception ex, IEventHandlerContainer events, HttpRequestMessage request)
        {
            HttpStatusCode code = HttpStatusCode.InternalServerError;
            Exception originalEx = ex;

            if (events?.CallbackAfterInternalServerError != null)
            {
                ex = events?.CallbackAfterInternalServerError(ex, ref code);
            }

            if (ex == null)
            {
                ex = new Exception("Exception is null.");
            }

            StringBuilder details = new StringBuilder();
            StringBuilder trace = new StringBuilder();
            var ex2 = ex;
            while (ex2.InnerException != null)
            {
                string detailsItem =
                    "{" +
                    $"{JsonConvert.ToString("code")}: {JsonConvert.ToString($"{(int)code}")}, " +
                    $"{JsonConvert.ToString("message")}: {JsonConvert.ToString(ex2.InnerException.Message)}" +
                    "}";
                if (details.Length > 0)
                    details.Append(", ");
                details.Append(detailsItem);
                ex2 = ex2.InnerException;
            }

            ex2 = ex;
            do
            {
                string traceItem =
                    "{" +
                    $"{JsonConvert.ToString("message")}: {JsonConvert.ToString(ex2.Message)}, " +
                    $"{JsonConvert.ToString("stack")}: {JsonConvert.ToString(ex2.StackTrace)}" +
                    "}";
                if (trace.Length > 0)
                    trace.Append(", ");
                trace.Append(traceItem);
                ex2 = ex2.InnerException;
            }
            while (ex2 != null);

            details.Insert(0, "[").Append("]");
            trace.Insert(0, $"{{{JsonConvert.ToString("trace")}: [").Append("]}");

            HttpResponseMessage msg = request.CreateResponse(code);
            msg.Content = new StringContent(
                "{" +
                $"{JsonConvert.ToString("error")}: " +
                "{ " +
                $"{JsonConvert.ToString("code")}: {JsonConvert.ToString($"{(int)code}")}, " +
                $"{JsonConvert.ToString("message")}: {JsonConvert.ToString(ex.Message)}, " +
                $"{JsonConvert.ToString("details")}: {details.ToString()}, " +
                $"{JsonConvert.ToString("innererror")}: {trace.ToString()}" +
                "}" +
                "}",
                Encoding.UTF8,
                "application/json");
            LogService.LogError(originalEx.Message, originalEx);
            return msg;
        }
#endif

#if NETFRAMEWORK
        private HttpResponseMessage TestPreferMinimal()
        {
            if (Request.Headers.Contains("Prefer"))
            {
                KeyValuePair<string, IEnumerable<string>> header = Request.Headers.FirstOrDefault(h => h.Key.ToLower() == "prefer");
                if (header.Value != null && header.Value.ToString().ToLower().Contains("return=minimal"))
                {
                    HttpResponseMessage result = Request.CreateResponse(HttpStatusCode.NoContent);
                    result.Headers.Add("Preference-Applied", "return=minimal");
                    return result;
                }
            }

            return null;
        }
#elif NETSTANDARD

        private NoContentResult TestPreferMinimal()
        {
            if (Request.Headers.ContainsKey("Prefer"))
            {
                KeyValuePair<string, StringValues> header = Request.Headers.FirstOrDefault(h => h.Key.ToLower() == "prefer");
                if (header.Value.ToString() != null && header.Value.ToString().ToLower().Contains("return=minimal"))
                {
                    NoContentResult result = NoContent();
                    Request.Headers.Add("Preference-Applied", "return=minimal");

                    return result;
                }
            }

            return null;
        }
#endif

#if NETFRAMEWORK
        /// <summary>
        /// Заменяет в теле запроса представление навигационных свойств с Имя_Связи@odata.bind:null на представление Имя_Связи:null.
        /// </summary>
        /// <returns>Возвращается EdmEntityObject преобразованный из JSON-строки.</returns>
        private EdmEntityObject ReplaceOdataBindNull()
        {
            if (!Request.Properties.ContainsKey(ExtendedODataEntityDeserializer.OdataBindNull))
            {
                if (Request.Properties.ContainsKey(ExtendedODataEntityDeserializer.ReadException))
                    throw (Exception)Request.Properties[ExtendedODataEntityDeserializer.ReadException];
                throw new Exception("ReplaceOdataBindNull: edmEntity is null.");
            }

            Stream stream;

            string requestContentKey = PostPatchHandler.RequestContent;
            if (Request.Properties.ContainsKey(PostPatchHandler.PropertyKeyBatchRequest) && (bool)Request.Properties[PostPatchHandler.PropertyKeyBatchRequest] == true)
            {
                requestContentKey = PostPatchHandler.RequestContent + $"_{PostPatchHandler.PropertyKeyContentId}_{Request.Properties[PostPatchHandler.PropertyKeyContentId]}";
            }

            string json = (string)Request.Properties[requestContentKey];

            Dictionary<string, object> props =
                JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new JsonSerializerSettings() { FloatParseHandling = FloatParseHandling.Decimal });
            var keys = props.Keys.ToArray();
            var odataBindNullList = new List<string>();
            foreach (var key in keys)
            {
                var p = key.IndexOf("@odata.bind");
                if (p != -1 && props[key] == null)
                {
                    props.Remove(key);
                    var newKey = key.Substring(0, p);
                    if (props.ContainsKey(newKey))
                    {
                        props.Remove(newKey);
                    }

                    var type = (EdmEntityTypeReference)Request.Properties[ExtendedODataEntityDeserializer.OdataBindNull];

                    var prop = type.FindNavigationProperty(newKey);
                    if (prop.Type.IsCollection())
                    {
                        odataBindNullList.Add(newKey);
                    }
                    else
                    {
                        props.Add(newKey, null);
                    }
                }
            }

            json = JsonConvert.SerializeObject(props);
            Request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            IContentNegotiator negotiator = (IContentNegotiator)Configuration.Services.GetService(typeof(IContentNegotiator));
            var resultNegotiate = negotiator.Negotiate(typeof(EdmEntityObject), Request, Configuration.Formatters);

            stream = Request.Content.ReadAsStreamAsync().Result;
            var formatter = resultNegotiate.Formatter;

            /*
            // Другой вариант получения форматтера.
                var formatter = ((ODataMediaTypeFormatter)Configuration.Formatters[0]).GetPerRequestFormatterInstance(
                    typeof(EdmEntityObject), Request, Request.Content.Headers.ContentType);

            */

            var edmEntity = (EdmEntityObject)formatter.ReadFromStreamAsync(
                typeof(EdmEntityObject),
                stream,
                Request.Content,
                new ModelStateFormatterLogger(ModelState, "edmEntity")).Result;
            if (edmEntity == null && Request.Properties.ContainsKey(ExtendedODataEntityDeserializer.ReadException))
            {
                throw (Exception)Request.Properties[ExtendedODataEntityDeserializer.ReadException];
            }

            foreach (var prop in odataBindNullList)
            {
                edmEntity.TrySetPropertyValue(prop, null);
            }

            return edmEntity;
        }
#elif NETSTANDARD
        /// <summary>
        /// Заменяет в теле запроса представление навигационных свойств с Имя_Связи@odata.bind:null на представление Имя_Связи:null.
        /// </summary>
        /// <returns>Возвращается EdmEntityObject преобразованный из JSON-строки.</returns>
        private EdmEntityObject ReplaceOdataBindNull()
        {
            if (!Request.HttpContext.Items.ContainsKey(ExtendedODataEntityDeserializer.OdataBindNull))
            {
                if (Request.HttpContext.Items.ContainsKey(ExtendedODataEntityDeserializer.ReadException))
                    throw (Exception)Request.HttpContext.Items[ExtendedODataEntityDeserializer.ReadException];
                throw new Exception("ReplaceOdataBindNull: edmEntity is null.");
            }

            string json = (string)Request.HttpContext.Items[RequestHeadersHookMiddleware.PropertyKeyRequestContent];

            Dictionary<string, object> props = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            var keys = props.Keys.ToArray();
            var odataBindNullList = new List<string>();
            foreach (var key in keys)
            {
                var p = key.IndexOf("@odata.bind");
                if (p != -1 && props[key] == null)
                {
                    props.Remove(key);
                    var newKey = key.Substring(0, p);
                    if (props.ContainsKey(newKey))
                    {
                        props.Remove(newKey);
                    }

                    var type = (EdmEntityTypeReference)Request.HttpContext.Items[ExtendedODataEntityDeserializer.OdataBindNull];

                    var prop = type.FindNavigationProperty(newKey);
                    if (prop.Type.IsCollection())
                    {
                        odataBindNullList.Add(newKey);
                    }
                    else
                    {
                        props.Add(newKey, null);
                    }
                }
            }

            json = JsonConvert.SerializeObject(props);
            Request.Body = new StringContent(json, Encoding.UTF8, "application/json").ReadAsStreamAsync().Result;

            var ictx = new InputFormatterContext(
                HttpContext,
                string.Empty,
                ModelState,
                MetadataProvider.GetMetadataForType(typeof(EdmEntityObject)),
                (x, y) => new StreamReader(x, y));

            IList<ODataInputFormatter> formatters = ODataInputFormatterFactory.Create();

            // The JSON input formatter is the first formatter in the OData input formatters list.
            InputFormatterResult formatterResult = formatters.First().ReadRequestBodyAsync(ictx, Encoding.UTF8).Result;

            var edmEntity = (EdmEntityObject)formatterResult.Model;

            if (edmEntity == null && Request.HttpContext.Items.ContainsKey(ExtendedODataEntityDeserializer.ReadException))
            {
                throw (Exception)Request.HttpContext.Items[ExtendedODataEntityDeserializer.ReadException];
            }

            foreach (var prop in odataBindNullList)
            {
                edmEntity.TrySetPropertyValue(prop, null);
            }

            return edmEntity;
        }
#endif

        /// <summary>
        /// Общая логика модификации данных: вставка и обновление в зависимости от статуса.
        /// Используется в Post (вставка) и Patch (обновление).
        /// </summary>
        /// <param name="edmEntity"> Модифицируемая сущность. </param>
        /// <param name="key"> Ключ сущности. Использовать, если не задан в сущности, но специфичен (не д.б. сгенерирован). </param>
        /// <returns> Созданная сущность. </returns>
        private DataObject UpdateObject(EdmEntityObject edmEntity, object key)
        {
            Init();

            // Список объектов для обновления.
            List<DataObject> objs = new List<DataObject>();

            try
            {
                // Создадим объект данных по пришедшей сущности.
                // В переменной objs сформируем список всех объектов для обновления в нужном порядке: сам объект и зависимые всех уровней.
                DataObject obj = GetDataObjectByEdmEntity(edmEntity, key, objs);

                for (int i = 0; i < objs.Count; i++)
                {
                    ObjectStatus status = objs[i].GetStatus(false);
                    if (status == ObjectStatus.Created)
                    {
                        if (!ExecuteCallbackBeforeCreate(objs[i]))
                        {
                            objs.RemoveAt(i);
                            i++;
                        }
                    }
                    else
                    {
                        if (!ExecuteCallbackBeforeUpdate(objs[i]))
                        {
                            objs.RemoveAt(i);
                            i++;
                        }
                    }
                }

                if (!OfflineManager.UnlockObjects(QueryOptions, objs))
                    throw new OperationCanceledException(); // TODO

                // Обработка объектов данных в хранилище средствами сервиса данных.
                // Статусы объектов должны автоматически получиться верными, т.к. в GetDataObjectByEdmEntity объект создаем
                // только при неудачной попытке вычитки и лишь затем инициализируем свойства пришедшими значениями.
                var objsArr = objs.ToArray();

                // Список объектов для обновления без UnAltered.
                var objsArrSmall = objsArr.Where(t => t.GetStatus() != ObjectStatus.UnAltered).ToArray();
                if (IsBatchChangeSetRequest)
                {
#if NETFRAMEWORK
                    List<DataObject> dataObjectsToUpdate = (List<DataObject>)Request.Properties[DataObjectODataBatchHandler.DataObjectsToUpdatePropertyKey];
#elif NETSTANDARD
                    List<DataObject> dataObjectsToUpdate = (List<DataObject>)Request.HttpContext.Items[DataObjectODataBatchHandler.DataObjectsToUpdatePropertyKey];
#endif
                    dataObjectsToUpdate.AddRange(objsArrSmall);
                }
                else
                {
                    _dataService.UpdateObjects(ref objsArrSmall);
                }

                // При успешном обновлении вычищаем из файловой системы, файлы подлежащие удалению.
                _dataObjectFileAccessor.RemoveFileUploadDirectories(_removingFileDescriptions);

                return obj;
            }
            catch (Exception)
            {
                _removingFileDescriptions.Clear();
                throw;
            }
        }

        /// <summary>
        /// Получить объект данных по ключу: если объект есть в хранилище, то возвращается загруженным по представлению по умолчанию, иначе - создаётся новый.
        /// </summary>
        /// <param name="objType">Тип объекта, не может быть <c>null</c>.</param>
        /// <param name="keyValue">Значение ключа.</param>>
        /// <returns>Объект данных.</returns>
        private DataObject ReturnDataObject(Type objType, object keyValue)
        {
            if (objType == null)
            {
                throw new ArgumentNullException(nameof(objType));
            }

            if (keyValue != null)
            {
                DataObject dataObjectFromCache = DataObjectCache.GetLivingDataObject(objType, keyValue);
                View view = _model.GetDataObjectDefaultView(objType);

                if (dataObjectFromCache != null)
                {
                    // Если объект не новый и не загружен целиком (начиная с ORM@5.1.0-beta15).
                    if (dataObjectFromCache.GetStatus(false) == ObjectStatus.UnAltered
                        && dataObjectFromCache.GetLoadingState() != LoadingState.Loaded)
                    {
                        // Для обратной совместимости сравним перечень загруженных свойств и свойств в представлении.
                        // TODO: удалить эту проверку после стабилизации версии 5.1.0.
                        string[] loadedProps = dataObjectFromCache.GetLoadedProperties();
                        IEnumerable<PropertyInView> ownProps = view.Properties.Where(p => !p.Name.Contains('.'));
                        if (!ownProps.All(p => loadedProps.Contains(p.Name)))
                        {
                            _dataService.LoadObject(view, dataObjectFromCache, true, true, DataObjectCache);
                        }
                    }

                    return dataObjectFromCache;
                }

                // Вычитывать объект сразу с детейлами нельзя, поскольку в этой же транзакции могут уже оказать отдельные операции с детейлами и перевычитка затрёт эти изменения.
                View lightView = view.Clone();
                DetailInView[] lightViewDetails = lightView.Details;
                foreach (DetailInView detailInView in lightViewDetails)
                {
                    lightView.RemoveDetail(detailInView.Name);
                }

                // Проверим существование объекта в базе.
                LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(objType, lightView);
                lcs.LimitFunction = FunctionBuilder.BuildEquals(keyValue);
                lcs.ReturnTop = 2;
                DataObject[] dobjs = _dataService.LoadObjects(lcs, DataObjectCache);
                if (dobjs.Length == 1)
                {
                    DataObject dataObject = dobjs[0];
                    if (lightViewDetails.Any())
                    {
                        // Дочитаем детейлы, чтобы в бизнес-серверах эти данные уже были. Детейлы с изменёнными состояниями будут пропущены из зачитки.
                        _dataService.SafeLoadDetails(view, new DataObject[] { dataObject }, DataObjectCache);
                    }

                    return dataObject;
                }
            }

            // Значение ключа автоматически создаётся.
            DataObject obj;

            if (keyValue != null)
            {
                obj = DataObjectCache.CreateDataObject(objType, keyValue);
            }
            else
            {
                obj = (DataObject)Activator.CreateInstance(objType);
                DataObjectCache.AddDataObject(obj);
            }

            return obj;
        }

        /// <summary>
        /// Построение объекта данных по сущности OData.
        /// </summary>
        /// <param name="edmEntity"> Сущность OData. </param>
        /// <param name="key"> Значение ключевого поля сущности. </param>
        /// <param name="dObjs"> Список объектов для обновления. </param>
        /// <param name="endObject"> Признак, что объект добавляется в конец списка обновления. </param>
        /// <returns> Объект данных. </returns>
        private DataObject GetDataObjectByEdmEntity(EdmEntityObject edmEntity, object key, List<DataObject> dObjs, bool endObject = false)
        {
            if (edmEntity == null)
            {
                return null;
            }

            IEdmEntityType entityType = (IEdmEntityType)edmEntity.ActualEdmType;
            Type objType = _model.GetDataObjectType(_model.GetEdmEntitySet(entityType).Name);

            // Значение свойства.
            object value;

            // Получим значение ключа.
            IEnumerable<IEdmProperty> entityProps = entityType.Properties().ToList();
            var keyProperty = entityProps.FirstOrDefault(prop => prop.Name == _model.KeyPropertyName);
            if (key != null)
            {
                value = key;
            }
            else
            {
                edmEntity.TryGetPropertyValue(keyProperty.Name, out value);
            }

            // Загрузим объект из хранилища, если он там есть (используем представление по умолчанию), или создадим, если нет, но только для POST.
            // Тем самым гарантируем загруженность свойств при необходимости обновления и установку нужного статуса.
            DataObject obj = ReturnDataObject(objType, value);

            // Добавляем объект в список для обновления, если там ещё нет объекта с таким ключом.
            var objInList = dObjs.FirstOrDefault(o => PKHelper.EQDataObject(o, obj, false));
            if (objInList == null)
            {
                if (!endObject)
                {
                    // Добавляем объект в начало списка.
                    dObjs.Insert(0, obj);
                }
                else
                {
                    // Добавляем в конец списка.
                    dObjs.Add(obj);
                }
            }

            // Все свойства объекта данных означим из пришедшей сущности, если они были там установлены(изменены).
            string agregatorPropertyName = Information.GetAgregatePropertyName(objType);
            IEnumerable<string> changedPropNames = edmEntity.GetChangedPropertyNames();

            // Обрабатываем агрегатор первым.
            List<IEdmProperty> changedProps = entityProps
                .Where(ep => changedPropNames.Contains(ep.Name))
                .OrderBy(ep => ep.Name != agregatorPropertyName)
                .ToList();
            foreach (var prop in changedProps)
            {
                string dataObjectPropName;
                try
                {
                    dataObjectPropName = _model.GetDataObjectProperty(entityType.FullTypeName(), prop.Name).Name;
                }
                catch (KeyNotFoundException)
                {
                    // Check if prop value is the link from master to pseudodetail (pseudoproperty).
                    if (HasPseudoproperty(entityType, prop.Name))
                    {
                        continue;
                    }

                    throw;
                }

                // Обработка мастеров и детейлов.
                if (prop is EdmNavigationProperty navProp)
                {
                    edmEntity.TryGetPropertyValue(prop.Name, out value);

                    EdmMultiplicity edmMultiplicity = navProp.TargetMultiplicity();

                    // Обработка мастеров.
                    if (edmMultiplicity == EdmMultiplicity.One || edmMultiplicity == EdmMultiplicity.ZeroOrOne)
                    {
                        if (value is EdmEntityObject edmMaster)
                        {
                            // Порядок вставки влияет на порядок отправки объектов в UpdateObjects это в свою очередь влияет на то, как срабатывают бизнес-серверы. Бизнес-сервер мастера должен сработать после, а агрегатора перед этим объектом.
                            bool insertIntoEnd = string.IsNullOrEmpty(agregatorPropertyName);
                            DataObject master = GetDataObjectByEdmEntity(edmMaster, null, dObjs, insertIntoEnd);

                            Information.SetPropValueByName(obj, dataObjectPropName, master);

                            if (dataObjectPropName == agregatorPropertyName)
                            {
                                master.AddDetail(obj);
                            }
                        }
                        else
                        {
                            Information.SetPropValueByName(obj, dataObjectPropName, null);
                        }
                    }

                    // Обработка детейлов.
                    if (edmMultiplicity == EdmMultiplicity.Many)
                    {
                        DetailArray detarr = (DetailArray)Information.GetPropValueByName(obj, dataObjectPropName);

                        if (value is EdmEntityObjectCollection coll)
                        {
                            if (coll != null && coll.Count > 0)
                            {
                                foreach (var edmEnt in coll)
                                {
                                    DataObject det = GetDataObjectByEdmEntity(
                                        (EdmEntityObject)edmEnt,
                                        null,
                                        dObjs,
                                        true);

                                    if (det.__PrimaryKey == null)
                                    {
                                        detarr.AddObject(det);
                                    }
                                    else
                                    {
                                        detarr.SetByKey(det.__PrimaryKey, det);
                                    }
                                }
                            }
                        }
                        else
                        {
                            detarr.Clear();
                        }
                    }
                }
                else
                {
                    // Обработка собственных свойств объекта (неключевых, т.к. ключ устанавливаем при начальной инициализации объекта obj).
                    if (prop.Name != keyProperty.Name)
                    {
                        Type dataObjectPropertyType = Information.GetPropertyType(objType, dataObjectPropName);
                        edmEntity.TryGetPropertyValue(prop.Name, out value);

                        // Если тип свойства относится к одному из зарегистрированных провайдеров файловых свойств,
                        // значит свойство файловое, и его нужно обработать особым образом.
                        if (_dataObjectFileAccessor.HasDataObjectFileProvider(dataObjectPropertyType))
                        {
                            IDataObjectFileProvider dataObjectFileProvider = _dataObjectFileAccessor.GetDataObjectFileProvider(dataObjectPropertyType);

                            // Обработка файловых свойств объектов данных.
                            string serializedFileDescription = value as string;
                            if (serializedFileDescription == null)
                            {
                                // Файловое свойство было сброшено на клиенте.
                                // Ассоциированный файл должен быть удален, после успешного сохранения изменений.
                                // Для этого запоминаем метаданные ассоциированного файла, до того как свойство будет сброшено
                                // (для получения метаданных свойство будет дочитано в объект данных).
                                // Файловое свойство типа File хранит данные ассоциированного файла прямо в БД,
                                // соответственно из файловой системы просто нечего удалять,
                                // поэтому обходим его стороной, чтобы избежать лишных вычиток файлов из БД.
                                if (dataObjectPropertyType != typeof(File))
                                {
                                    var fileDescription = dataObjectFileProvider.GetFileDescription(_dataService, obj, dataObjectPropName);
                                    _removingFileDescriptions.Add(fileDescription);
                                }

                                // Сбрасываем файловое свойство в изменяемом объекте данных.
                                Information.SetPropValueByName(obj, dataObjectPropName, null);
                            }
                            else
                            {
                                // Файловое свойство было изменено, но не сброшено.
                                // Если в метаданных файла присутствует FileUploadKey значит файл был загружен на сервер,
                                // но еще не был ассоциирован с объектом данных, и это нужно сделать.
                                FileDescription fileDescription = FileDescription.FromJson(serializedFileDescription);
                                fileDescription.FilePropertyType = dataObjectPropertyType;
                                if (!(string.IsNullOrEmpty(fileDescription.FileUploadKey) || string.IsNullOrEmpty(fileDescription.FileName)))
                                {
                                    var fileProperty = dataObjectFileProvider.GetFileProperty(_dataService, fileDescription);
                                    Information.SetPropValueByName(obj, dataObjectPropName, fileProperty);

                                    // Файловое свойство типа File хранит данные ассоциированного файла прямо в БД,
                                    // поэтому после успешного сохранения объекта данных, оссоциированный с ним файл должен быть удален из файловой системы.
                                    // Для этого запоминаем описание загруженного файла.
                                    if (dataObjectPropertyType == typeof(File))
                                    {
                                        _removingFileDescriptions.Add(fileDescription);
                                    }
                                }
                            }
                        }
                        else
                        {
                            // Преобразование типов для примитивных свойств.
                            if (value is DateTimeOffset)
                                value = ((DateTimeOffset)value).UtcDateTime;
                            if (value is EdmEnumObject)
                                value = ((EdmEnumObject)value).Value;

                            Information.SetPropValueByName(obj, dataObjectPropName, value);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(agregatorPropertyName))
            {
                DataObject agregator = (DataObject)Information.GetPropValueByName(obj, agregatorPropertyName);

                if (agregator != null)
                {
                    DataObject existObject = dObjs.FirstOrDefault(o => PKHelper.EQDataObject(o, agregator, false));
                    if (existObject == null)
                    {
                        if (!endObject)
                        {
                            // Добавляем объект в начало списка.
                            dObjs.Insert(0, agregator);
                        }
                        else
                        {
                            // Добавляем в конец списка.
                            dObjs.Add(agregator);
                        }
                    }
                }
            }

            return obj;
        }
    }
}
