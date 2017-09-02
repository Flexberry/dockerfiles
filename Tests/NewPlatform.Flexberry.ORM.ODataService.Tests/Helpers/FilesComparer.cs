namespace NewPlatform.Flexberry.ORM.ODataService.Tests.Helpers
{
    using System;
    using System.IO;

    /// <summary>
    /// Вспомогательный класс, предоставляющий методы для сравнения файлов между собой.
    /// </summary>
    public class FilesComparer
    {
        /// <summary>
        /// Осуществляет проверку того, что два файла равны между собой.
        /// </summary>
        /// <param name="filePath1">Путь к первому файлу.</param>
        /// <param name="filePath2">Путь ко второму файлу.</param>
        /// <returns>Флаг: <c>true</c>, если файлы равны между собой, <c>false</c> в противном случае.</returns>
        public static bool FilesAreEqual(string filePath1, string filePath2)
        {
            return FilesAreEqual(new FileInfo(filePath1), new FileInfo(filePath2));
        }

        /// <summary>
        /// Осуществляет проверку того, что два файла равны между собой.
        /// </summary>
        /// <param name="fileInfo1">Информация о первом файле.</param>
        /// <param name="fileInfo2">Информация о втором файле.</param>
        /// <param name="bufferSize">Размер порций, которыми будет осуществляться вычитка и сравнения файлов.</param>
        /// <returns>Флаг: <c>true</c>, если файлы равны между собой, <c>false</c> в противном случае.</returns>
        public static bool FilesAreEqual(FileInfo fileInfo1, FileInfo fileInfo2, int bufferSize = 8192)
        {
            if (!fileInfo1.Exists && !fileInfo2.Exists)
            {
                throw new FileNotFoundException("Files are not found.");
            }

            if (fileInfo1.Exists != fileInfo2.Exists)
            {
                return false;
            }

            if (fileInfo1.Name != fileInfo2.Name)
            {
                return false;
            }

            if (fileInfo1.Length != fileInfo2.Length)
            {
                return false;
            }

            byte[] buffer1 = new byte[bufferSize];
            byte[] buffer2 = new byte[bufferSize];

            using (FileStream stream1 = fileInfo1.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (FileStream stream2 = fileInfo2.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    while (true)
                    {
                        var bytesRead1 = stream1.Read(buffer1, 0, bufferSize);
                        var bytesRead2 = stream2.Read(buffer2, 0, bufferSize);

                        if (bytesRead1 != bytesRead2)
                        {
                            return false;
                        }

                        if (bytesRead1 == 0)
                        {
                            return true;
                        }

                        if (!ArraysAreEqual(buffer1, buffer2, bytesRead1))
                        {
                            return false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Осуществляет проверку того, что два массива байтов равны между собой.
        /// </summary>
        /// <param name="array1">Первый массив байтов.</param>
        /// <param name="array2">Второй массив байтов.</param>
        /// <param name="bytesToCompare">Количество байтов для сравнения.</param>
        /// <returns>Флаг: <c>true</c>, если массивы равны между собой, <c>false</c> в противном случае.</returns>
        public static bool ArraysAreEqual(byte[] array1, byte[] array2, int bytesToCompare = 0)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            int length = (bytesToCompare == 0) ? array1.Length : bytesToCompare;
            int tailIdx = length - (length % sizeof(long));

            for (int i = 0; i < tailIdx; i += sizeof(long))
            {
                if (BitConverter.ToInt64(array1, i) != BitConverter.ToInt64(array2, i))
                {
                    return false;
                }
            }

            for (int i = tailIdx; i < length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
