using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using System.Diagnostics;

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

        public static List<DatImage> LoadImagesFromSubdirectory(string subdirectoryPath)
        {
            List<DatImage> images = new List<DatImage>();
            string imagePath = Path.Combine(subdirectoryPath, "Image");

            if (Directory.Exists(imagePath))
            {
                foreach (var yearMonthDir in Directory.GetDirectories(imagePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(yearMonthDir);

                    foreach (FileInfo file in directoryInfo.GetFiles("*.dat"))
                    {
                        Debug.WriteLine("try to decrypt " + file.FullName);
                        Image image;
                        string imageType;
                        (image, imageType) = DecryptImage(file.FullName);
                        images.Add(new DatImage(image, file.FullName, imageType));
                    }
                }
            }

            return images;
        }

        public static (Image, string) DecryptImage(string filePath)
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
                return (Image.FromStream(ms), fileType);
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
    }
}
