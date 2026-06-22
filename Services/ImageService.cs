using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LibraryApp.Services
{
    /// <summary>
    /// Загрузка изображений по URL или локальному пути (из поля "фото" таблицы Книги).
    /// Кеш в памяти — одна ссылка грузится только один раз за сессию.
    /// </summary>
    public static class ImageService
    {
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        // null = уже пытались загрузить и не получилось
        private static readonly Dictionary<string, Image?> Cache =
            new(StringComparer.OrdinalIgnoreCase);

        private static readonly Lazy<Image> Placeholder =
            new(CreatePlaceholder);

        public static Image PlaceholderImage => Placeholder.Value;

        /// <summary>
        /// Асинхронная загрузка изображения по URL или локальному пути.
        /// </summary>
        public static async Task<Image?> LoadAsync(string? urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath))
                return null;

            string path = urlOrPath
                .Trim()
                .Trim('"')
                .Trim('\'')
                .Trim();

            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (Cache.TryGetValue(path, out Image? cached))
                return cached;

            Image? image = null;

            try
            {
                // HTTP / HTTPS
                if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] bytes = await HttpClient.GetByteArrayAsync(path);

                    using var ms = new MemoryStream(bytes);
                    using var temp = Image.FromStream(ms);

                    image = new Bitmap(temp);
                }
                else
                {
                    // Локальный путь
                    string normalized = path.Replace('/', '\\');

                    // Если путь относительный (например Photos\abc.jpg)
                    if (!Path.IsPathRooted(normalized))
                    {
                        normalized = Path.GetFullPath(
                            Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory,
                                "..",
                                "..",
                                "..",
                                "..",
                                normalized));
                    }

                    if (File.Exists(normalized))
                    {
                        byte[] bytes = await File.ReadAllBytesAsync(normalized);

                        using var ms = new MemoryStream(bytes);
                        using var temp = Image.FromStream(ms);

                        image = new Bitmap(temp);
                    }
                }
            }
            catch
            {
                image = null;
            }

            Cache[path] = image;
            return image;
        }

        /// <summary>
        /// Масштабирование изображения с сохранением пропорций.
        /// </summary>
        public static Image Resize(Image? source, int width, int height)
        {
            var result = new Bitmap(width, height);

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
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(source.Width * ratio);
            int newHeight = (int)(source.Height * ratio);

            g.DrawImage(
                source,
                (width - newWidth) / 2,
                (height - newHeight) / 2,
                newWidth,
                newHeight);

            return result;
        }

        private static Image CreatePlaceholder()
        {
            var bmp = new Bitmap(60, 56);

            using var g = Graphics.FromImage(bmp);

            DrawPlaceholder(g, 60, 56);

            return bmp;
        }

        private static void DrawPlaceholder(Graphics g, int width, int height)
        {
            g.Clear(Color.FromArgb(220, 220, 220));

            using var pen =
                new Pen(Color.FromArgb(180, 180, 180), 1);

            g.DrawRectangle(
                pen,
                0,
                0,
                width - 1,
                height - 1);

            using var brush =
                new SolidBrush(Color.FromArgb(150, 150, 150));

            int bx = width / 2 - 10;
            int by = height / 2 - 12;

            g.FillRectangle(brush, bx, by, 20, 24);

            using var whiteBrush =
                new SolidBrush(Color.FromArgb(235, 235, 235));

            g.FillRectangle(
                whiteBrush,
                bx + 3,
                by + 3,
                14,
                18);

            g.DrawLine(pen, bx + 3, by + 7, bx + 17, by + 7);
            g.DrawLine(pen, bx + 3, by + 11, bx + 17, by + 11);
            g.DrawLine(pen, bx + 3, by + 15, bx + 13, by + 15);
        }

        public static void ClearCache()
        {
            foreach (var image in Cache.Values)
            {
                image?.Dispose();
            }

            Cache.Clear();
        }
    }
}