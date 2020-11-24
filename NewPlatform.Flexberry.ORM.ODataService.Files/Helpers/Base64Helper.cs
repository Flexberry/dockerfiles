namespace NewPlatform.Flexberry.ORM.ODataService.Files.Helpers
{
    using System;
    using System.IO;

    public class Base64Helper
    {
        /// <summary>
        /// Осуществляет получение данных файла в виде Base64String.
        /// </summary>
        /// <param name="contentType">MIME-тип данных.</param>
        /// <param name="stream">Поток байтов файла.</param>
        /// <returns>Данные файла в виде  Base64String.</returns>
        public static string GetBase64StringFileData(string contentType, Stream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, (int)stream.Length);

            return string.Format("data:{0};base64,{1}", contentType, Convert.ToBase64String(buffer));
        }

        /// <summary>
        /// Осуществляет получение данных файла в виде Base64String.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        /// <returns>Данные файла в виде  Base64String.</returns>
        public static string GetBase64StringFileData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return string.Empty;
            }

            FileInfo fileInfo = new FileInfo(filePath);
            using var fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            string result = GetBase64StringFileData(MimeTypeUtils.GetFileMimeType(fileInfo.Name), fileStream);

            return result;
        }
    }
}
