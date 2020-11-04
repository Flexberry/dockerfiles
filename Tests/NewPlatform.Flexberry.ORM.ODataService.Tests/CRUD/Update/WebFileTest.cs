namespace NewPlatform.Flexberry.ORM.ODataService.Tests.CRUD.Update
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using ICSSoft.Services;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.UserDataTypes;

    using NewPlatform.Flexberry.ORM.ODataService.Tests.Extensions;
    using Unity;
    using Xunit;

    public class WebFileTest: BaseODataServiceIntegratedTest
    {
#if NETCOREAPP
        /// <summary>
        /// Конструктор по-умолчанию.
        /// </summary>
        /// <param name="factory">Фабрика для приложения.</param>
        /// <param name="output">Вывод отладочной информации.</param>
        public WebFileTest(CustomWebApplicationFactory<ODataServiceSample.AspNetCore.Startup> factory, Xunit.Abstractions.ITestOutputHelper output)
            : base(factory, output)
        {
        }
#endif

        [Fact]
        public void WebFileAsStringShouldSave()
        {
            ActODataService(args =>
            {
                var медведь = new Медведь();
                медведь.Берлога.Add(new Берлога());

                args.DataService.UpdateObject(медведь);

                медведь.Берлога[0].Наименование = "Элитная берлога №1";
                string сертификатСтрока = @"{\""fileUrl\"":\""http://localhost:6500/api/File?fileUploadKey=69ac85f7-c472-44bf-be43-c33f6d79a625&fileName=cert.txt\"",\""previewUrl\"":\""http://localhost:6500/api/File?fileUploadKey=69ac85f7-c472-44bf-be43-c33f6d79a625&fileName=cert.txt&getPreview=true\"",\""fileName\"":\""cert.txt\"",\""fileSize\"":2607,\""fileMimeType\"":\""application/octet-stream\""}";
                медведь.Берлога[0].СертификатСтрока = сертификатСтрока;

                var view = new View { DefineClassType = typeof(Берлога) };
                view.AddProperties(
                    Information.ExtractPropertyName<Берлога>(b => b.__PrimaryKey),
                    Information.ExtractPropertyName<Берлога>(b => b.Наименование),
                    Information.ExtractPropertyName<Берлога>(b => b.СертификатСтрока));

                const string baseUrl = "http://localhost/odata";

                string json = медведь.Берлога[0].ToJson(view, args.Token.Model);

                var changesets = new[]
                {
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(Медведь)).Name}",
                        "{}",
                        медведь),
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name}",
                        json,
                        медведь.Берлога[0]),
                };
                var batchRequest = CreateBatchRequest(baseUrl, changesets);
                using (var response = args.HttpClient.SendAsync(batchRequest).Result)
                {
                    CheckODataBatchResponseStatusCode(response, new[] { HttpStatusCode.OK, HttpStatusCode.OK });

                    args.DataService.LoadObject(Медведь.Views.МедведьE, медведь);

                    var берлога = медведь.Берлога.Cast<Берлога>().FirstOrDefault();

                    Assert.NotNull(берлога);
                    Assert.Equal(сертификатСтрока, берлога.СертификатСтрока);
                }
            });
        }

        /// <summary>
        /// Тест проверяющий сохранение webfile'a в batch запросе.
        /// </summary>
        [Fact]
        public void WebFileShouldSave()
        {
            ActODataService(async args =>
            {
                var медведь = new Медведь();
                args.DataService.UpdateObject(медведь);

                string key = Guid.NewGuid().ToString("D");
                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), key));
                const string fileName = "cert.txt";
                string basePath = Path.GetTempPath();
#if NETCOREAPP
                Unity.IUnityContainer container = UnityFactory.GetContainer();
                var env = container.Resolve<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                basePath = Path.Combine(env.WebRootPath, "Uploads");
                if (!Directory.Exists(Path.Combine(basePath, key)))
                {
                    Directory.CreateDirectory(Path.Combine(basePath, key));
                }
#endif
                string filePath = Path.Combine(basePath, key, fileName);
                using (var fs = new FileStream(filePath, FileMode.Create))
                {
                    fs.SetLength(100);
                }

                string сертификатСтрока = $@"{{\""fileUrl\"":\""http://localhost:6500/api/File?fileUploadKey={key}&fileName={fileName}\"",\""previewUrl\"":\""http://localhost:6500/api/File?fileUploadKey={key}&fileName={fileName}&getPreview=true\"",\""fileName\"":\""{fileName}\"",\""fileSize\"":100,\""fileMimeType\"":\""application/octet-stream\""}}";

                var берлога = new Берлога
                {
                    Наименование = "Элитная берлога №1",
                    Сертификат = new WebFile
                    {
                        Name = fileName,
                    },
                    Медведь = медведь
                };

                var view = new View { DefineClassType = typeof(Берлога) };
                view.AddProperties(
                    Information.ExtractPropertyName<Берлога>(b => b.__PrimaryKey),
                    Information.ExtractPropertyName<Берлога>(b => b.Медведь),
                    Information.ExtractPropertyName<Берлога>(b => b.Наименование),
                    Information.ExtractPropertyName<Берлога>(b => b.Сертификат));

                const string baseUrl = "http://localhost/odata";

                string json = берлога.ToJson(view, args.Token.Model).Replace(fileName, сертификатСтрока); // TODO сериализовать корректно

                var changesets = new[]
                {
                    CreateChangeset(
                        $"{baseUrl}/{args.Token.Model.GetEdmEntitySet(typeof(Берлога)).Name}",
                        json,
                        берлога)
                };
                var batchRequest = CreateBatchRequest(baseUrl, changesets);
                using (var response = args.HttpClient.SendAsync(batchRequest).Result)
                {
                    CheckODataBatchResponseStatusCode(response, new[] { HttpStatusCode.Created });

                    args.DataService.LoadObject(Медведь.Views.МедведьE, медведь);

                    var берлога2 = медведь.Берлога.Cast<Берлога>().FirstOrDefault();

                    Assert.NotNull(берлога2);
                    Assert.NotNull(берлога2.Сертификат);
                }

                File.Delete(filePath);
            });
        }
    }
}
