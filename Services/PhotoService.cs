using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace LibraryApp.Services
{
    /// <summary>
    /// Сохранение фото книги: приведение к 300×200 px, хранение пути в БД,
    /// удаление старого файла при замене (Задание 3.3).
    /// </summary>
    public static class PhotoService
    {
        private const int TargetWidth = 300;
        private const int TargetHeight = 200;

        public static string PhotosFolder
        {
            get
            {
                string projectFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
                string photos = Path.Combine(projectFolder, "Photos");
                Directory.CreateDirectory(photos);
                return photos;
            }
        }

        /// <summary>
        /// Копирует выбранный пользователем файл, приводит к 300×200 px, сохраняет в папку Photos.
        /// Если был старый файл — удаляет его. Возвращает относительный путь для сохранения в БД.
        /// </summary>
        public static string SavePhoto(string sourceFilePath, string? oldRelativePath)
        {
            Directory.CreateDirectory(PhotosFolder);

            using var original = new Bitmap(sourceFilePath);
            using var resized = new Bitmap(TargetWidth, TargetHeight);

            using (Graphics g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(original, 0, 0, TargetWidth, TargetHeight);
            }

            string fileName = $"{Guid.NewGuid():N}.jpg";
            string fullPath = Path.Combine(PhotosFolder, fileName);
            resized.Save(fullPath, ImageFormat.Jpeg);

            // Старое фото удаляем только после успешного сохранения нового
            DeletePhoto(oldRelativePath);

            return Path.Combine("Photos", fileName);
        }

        /// <summary>Удаляет файл фото по относительному пути из БД, если он существует.</summary>
        public static void DeletePhoto(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            string fullPath = Path.GetFullPath(Path.Combine(PhotosFolder, Path.GetFileName(relativePath)));

            if (File.Exists(fullPath))
            {
                try
                {
                    File.Delete(fullPath);
                }
                catch (IOException)
                {
                    // Файл занят/недоступен — не критично для основной операции, пропускаем.
                }
            }
        }

        /// <summary>Загружает изображение для предпросмотра (без блокировки исходного файла).</summary>
        public static Image LoadForPreview(string fullPath)
        {
            using var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            using var temp = Image.FromStream(stream);
            return new Bitmap(temp);
        }
    }
}
