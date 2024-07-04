using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace WechatCleanerPlus
{
    internal class ImageProcessor
    {
        private static byte[][] fileHeaders = new byte[][]
        {
            new byte[] { 0xFF, 0xD8, 0xFF }, // JPEG
            new byte[] { 0x89, 0x50, 0x4E, 0x47 }, // PNG
            new byte[] { 0x47, 0x49, 0x46, 0x38 }, // GIF
            new byte[] { 0x49, 0x49, 0x2A, 0x00 }, // TIFF
            new byte[] { 0x42, 0x4D } // BMP
        };
        private static string[] fileTypeNames = new string[]
        {
            "JPEG",
            "PNG",
            "GIF",
            "TIFF",
            "BMP"
        };

        public static List<DatImage> LoadImagesFromSubdirectory(string subdirectoryPath, CancellationToken cancellationToken)
        {
            List<DatImage> images = new List<DatImage>();
            string imagePath = Path.Combine(subdirectoryPath, "Image");

            if (Directory.Exists(imagePath))
            {
                foreach (var yearMonthDir in Directory.GetDirectories(imagePath))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    DirectoryInfo directoryInfo = new DirectoryInfo(yearMonthDir);

                    foreach (FileInfo file in directoryInfo.GetFiles("*.dat"))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Debug.WriteLine("try to decrypt " + file.FullName);
                        Image image;
                        string imageType;
                        (image, imageType) = DecryptImage(file.FullName, true);
                        images.Add(new DatImage(image, file.FullName, imageType));
                    }
                }
            }

            return images;
        }

        public static (Image, string) DecryptImage(string filePath, bool return_thumbnail=false)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            byte key;
            string fileType;
            (key, fileType) = FindDecryptionKey(fileBytes);
            if (key == 0) return (null, "UNKNOWN"); // 没有找到合适的密钥

            Debug.WriteLine("Find key: " + key);
            byte[] decryptedBytes = DecryptData(fileBytes, key);
            using (MemoryStream ms = new MemoryStream(decryptedBytes))
            {
                if (return_thumbnail == true)
                {
                    using (Image originalImage = Image.FromStream(ms))
                    {
                        // 创建缩放后的图像
                        Image resizedImage = ResizeImage(originalImage, 128, 128);
                        return (resizedImage, fileType);
                    }
                }
                else
                {
                    return (Image.FromStream(ms), fileType);
                }
            }
        }

        private static (byte, string) FindDecryptionKey(byte[] fileBytes)
        {
            for (int filetype = 0; filetype < 5; filetype++)
            {
                if ((fileBytes[0] ^ fileHeaders[filetype][0]) ==
                    (fileBytes[1] ^ fileHeaders[filetype][1]))
                {
                    return (((byte)(fileBytes[0] ^ fileHeaders[filetype][0])),
                            fileTypeNames[filetype]);
                }
            }

            return (0, "UNKNOWN"); // 没有找到密钥
        }

        private static byte[] DecryptData(byte[] fileBytes, byte key)
        {
            byte[] decryptedBytes = new byte[fileBytes.Length];
            for (int i = 0; i < fileBytes.Length; i++)
            {
                decryptedBytes[i] = (byte)(fileBytes[i] ^ key);
            }

            return decryptedBytes;
        }

        private static Image ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
