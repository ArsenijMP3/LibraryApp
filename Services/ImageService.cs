using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LibraryApp.Services
{
    /// <summary>
    /// Загрузка изображений по URL или локальному пути (из поля "фото" таблицы Книги).
    /// Кеш хранит байты — Bitmap пересоздаётся при каждом запросе через Clone(),
    /// что гарантирует полную независимость от исходного потока.
    /// </summary>
    public static class ImageService
    {
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // Кеш байтов: null = уже пробовали — не получилось
        private static readonly Dictionary<string, byte[]?> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly Lazy<Image> PlaceholderLazy = new(CreatePlaceholder);
        public static Image PlaceholderImage => PlaceholderLazy.Value;

        /// <summary>
        /// Асинхронно загружает изображение и возвращает независимый Bitmap.
        /// Bitmap.Clone(Rectangle, PixelFormat) копирует пиксели в новый объект,
        /// который не зависит от MemoryStream и не становится невалидным после её Dispose.
        /// </summary>
        public static async Task<Image?> LoadAsync(string? urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath))
                return null;

            string path = urlOrPath.Trim().Trim('"').Trim('\'').Trim();
            if (string.IsNullOrEmpty(path))
                return null;

            // Берём байты из кеша или загружаем
            byte[]? bytes;
            if (Cache.TryGetValue(path, out byte[]? cached))
            {
                bytes = cached;
            }
            else
            {
                bytes = await FetchBytesAsync(path);
                Cache[path] = bytes;
            }

            if (bytes == null)
                return null;

            return CreateIndependentBitmap(bytes);
        }

        /// <summary>
        /// Создаёт независимый Bitmap из байтов через Bitmap.Clone(Rectangle, PixelFormat).
        /// Это официальный способ получить копию пикселей без привязки к потоку.
        /// Graphics.DrawImage не используется — он ненадёжен при совместной работе
        /// с временными объектами под .NET 10.
        /// </summary>
        private static Bitmap? CreateIndependentBitmap(byte[] bytes)
        {
            MemoryStream? ms = null;
            Bitmap? source = null;
            try
            {
                ms = new MemoryStream(bytes);
                source = new Bitmap(ms);

                // Clone копирует все пиксели в новый объект Bitmap.
                // Результат не зависит ни от source, ни от ms — их можно безопасно Dispose.
                Bitmap result = source.Clone(
                    new Rectangle(0, 0, source.Width, source.Height),
                    PixelFormat.Format32bppArgb);

                return result;
            }
            catch
            {
                return null;
            }
            finally
            {
                // Порядок важен: сначала Bitmap, потом Stream
                source?.Dispose();
                ms?.Dispose();
            }
        }

        private static async Task<byte[]?> FetchBytesAsync(string path)
        {
            try
            {
                if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return await HttpClient.GetByteArrayAsync(path);
                }

                // Локальный файл — нормализуем разделители
                string normalized = path.Replace('/', '\\');

                // Относительный путь — ищем рядом с .exe
                if (!Path.IsPathRooted(normalized))
                {
                    normalized = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, normalized);
                }

                if (File.Exists(normalized))
                    return await File.ReadAllBytesAsync(normalized);

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Масштабирует изображение под размер ячейки (letterbox).</summary>
        public static Image Resize(Image? source, int width, int height)
        {
            // Создаём результирующий Bitmap
            var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using var g = Graphics.FromImage(result);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            if (source == null)
            {
                DrawPlaceholder(g, width, height);
                return result;
            }

            g.Clear(Color.White);

            double ratioX = (double)width / source.Width;
            double ratioY = (double)height / source.Height;
            double ratio  = Math.Min(ratioX, ratioY);
            int newW = Math.Max(1, (int)(source.Width  * ratio));
            int newH = Math.Max(1, (int)(source.Height * ratio));

            g.DrawImage(source,
                (width  - newW) / 2,
                (height - newH) / 2,
                newW, newH);

            return result;
        }

        private static Image CreatePlaceholder()
        {
            var bmp = new Bitmap(60, 56, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            DrawPlaceholder(g, 60, 56);
            return bmp;
        }

        private static void DrawPlaceholder(Graphics g, int w, int h)
        {
            g.Clear(Color.FromArgb(220, 220, 220));
            using var pen   = new Pen(Color.FromArgb(180, 180, 180), 1);
            using var dark  = new SolidBrush(Color.FromArgb(150, 150, 150));
            using var light = new SolidBrush(Color.FromArgb(235, 235, 235));

            g.DrawRectangle(pen, 0, 0, w - 1, h - 1);

            int bx = w / 2 - 10, by = h / 2 - 12;
            g.FillRectangle(dark,  bx,     by,     20, 24);
            g.FillRectangle(light, bx + 3, by + 3, 14, 18);
            g.DrawLine(pen, bx + 3, by + 7,  bx + 17, by + 7);
            g.DrawLine(pen, bx + 3, by + 11, bx + 17, by + 11);
            g.DrawLine(pen, bx + 3, by + 15, bx + 13, by + 15);
        }

        public static void ClearCache() => Cache.Clear();
    }
}
