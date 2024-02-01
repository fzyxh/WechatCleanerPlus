using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace WechatCleanerPlus
{
    internal class DatImage
    {
        public Image Image { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public DateTime ModifyTime => File.GetLastWriteTime(FilePath);

        public DatImage(Image image, string filePath, string fileType)
        {
            Image = image;
            FilePath = filePath;
            FileType = fileType;
        }
    }
}
