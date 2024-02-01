using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WechatCleanerPlus
{
    public partial class WechatCleanerPlus : Form
    {
        private ContextMenuStrip contextMenuStrip1;

        private string currentPath;
        private List<DatImage> datImages;

        public WechatCleanerPlus()
        {
            InitializeComponent();
            // 设置 SplitContainer 控件的 FixedPanel 属性
            splitContainer1.FixedPanel = FixedPanel.Panel1;
            this.Load += new System.EventHandler(this.MyForm_Load);
            this.listView1.ColumnClick += new ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.FullRowSelect = true;


            contextMenuStrip1 = new ContextMenuStrip();
            contextMenuStrip1.Items.Add("删除"); // 添加删除选项
            this.listView1.ContextMenuStrip = contextMenuStrip1; // 将 ContextMenuStrip 与 ListView 关联
            contextMenuStrip1.Items[0].Click += 删除ToolStripMenuItem_Click;
        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedItems();
        }

        private void MyForm_Load(object sender, EventArgs e)
        {
            listView1.View = View.Details;
            listView1.Columns.Clear();
            listView1.Columns.Add("对象名称", -2, HorizontalAlignment.Left);
            listView1.Columns.Add("目录名称", -2, HorizontalAlignment.Center);
            listView1.Columns.Add("总大小", -2, HorizontalAlignment.Right);
            int totalWidth = listView1.ClientRectangle.Width; // 获取ListView的客户端宽度
            int sizeColumnWidth = 100; // 将宽度平均分配给两列

            listView1.Columns[2].Width = sizeColumnWidth;
            listView1.Columns[0].Width = (totalWidth - sizeColumnWidth)/2;
            listView1.Columns[1].Width = totalWidth - sizeColumnWidth - listView1.Columns[0].Width;

            // 如果有默认的目录需要加载，可以在这里调用加载目录的方法
            // LoadDirectoriesToListView("默认目录路径");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = checkBox1.Checked;

            foreach (ListViewItem item in listView2.Items)
            {
                item.Selected = isChecked;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要导出的图片");
                return;
            }

            using (FolderBrowserDialog folderDlg = new FolderBrowserDialog())
            {
                if (folderDlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (ListViewItem item in listView2.SelectedItems)
                    {
                        DatImage datImage = item.Tag as DatImage;
                        string targetPath = Path.Combine(folderDlg.SelectedPath, 
                            datImage.FileName.Substring(0, datImage.FileName.Length - 3) + datImage.FileType.ToLower());

                        ImageFormat format = GetImageFormat(datImage.FileType); // 根据需要更改
                        Debug.WriteLine("try to save: " + targetPath);

                        Image decryptedImage;
                        string tmp;
                        (decryptedImage, tmp) = ImageProcessor.DecryptImage(datImage.FilePath);

                        if (decryptedImage != null)
                        {
                            using (Bitmap originalBitmap = new Bitmap(decryptedImage))
                            {
                                originalBitmap.Save(targetPath, format);
                            }
                        }
                    }

                    MessageBox.Show("导出完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static ImageFormat GetImageFormat(string fileType)
        {
            switch (fileType.ToUpper())
            {
                case "JPEG":
                    return ImageFormat.Jpeg;
                case "PNG":
                    return ImageFormat.Png;
                case "GIF":
                    return ImageFormat.Gif;
                case "TIFF":
                    return ImageFormat.Tiff;
                case "BMP":
                    return ImageFormat.Bmp;
                default:
                    throw new ArgumentException("未知的文件类型");
            }
        }

        private void WechatCleanerPlus_Load(object sender, EventArgs e)
        {

        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void 打开文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                currentPath = folderBrowserDialog1.SelectedPath;
                LoadDirectoriesToListView(currentPath);
            }
        }

        private void LoadDirectoriesToListView(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            listView1.Items.Clear();
            foreach (var dir in di.GetDirectories())
            {
                long size = GetDirectorySize(dir.FullName);
                string[] row = { dir.Name, dir.Name, FormatSize(size) };
                var listViewItem = new ListViewItem(row);
                listView1.Items.Add(listViewItem);
            }
        }

        private long GetDirectorySize(string folderPath)
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            return di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }

        private string FormatSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchTextBox.Text.ToLower();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadDirectoriesToListView(currentPath); // currentPath是当前主目录的路径
            }
            else
            {
                var filteredItems = listView1.Items.Cast<ListViewItem>()
                                    .Where(item => item.Text.ToLower().Contains(searchText))
                                    .ToArray();
                listView1.Items.Clear();
                listView1.Items.AddRange(filteredItems);
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // 点击不同的列时，切换排序顺序
            if (listView1.Sorting == SortOrder.Ascending)
                listView1.Sorting = SortOrder.Descending;
            else
                listView1.Sorting = SortOrder.Ascending;

            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column, listView1.Sorting);
            listView1.Sort();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count != 1) return;

            string selectedSubdirectory = Path.Combine(currentPath, listView1.SelectedItems[0].Text);
            Debug.WriteLine("select dir: " + selectedSubdirectory);

            if (datImages != null)
            {
                foreach (var img in datImages)
                {
                    img.Image.Dispose();
                }
            }

            datImages = ImageProcessor.LoadImagesFromSubdirectory(selectedSubdirectory);
            Debug.WriteLine("Image process complete!");

            DisplayImagesInListView();
        }

        private void DisplayImagesInListView()
        {
            imageList1.Images.Clear();
            imageList1.ImageSize = new Size(128, 128);
            listView2.Items.Clear();
            listView2.View = View.LargeIcon; // 或者使用 View.SmallIcon
            listView2.LargeImageList = imageList1; // 确保已经在Form Designer中创建了imageList1

            for (int i = 0; i < datImages.Count; i++)
            {
                imageList1.Images.Add(datImages[i].Image);
                string formatString = datImages[i].FileType.ToLower();
                listView2.Items.Add(new ListViewItem
                {
                    ImageIndex = i,
                    Text = datImages[i].FileName.Substring(0, datImages[i].FileName.Length - 3)
                           + formatString, // 或者使用其他描述性文本
                    Tag = datImages[i] // 在这里设置 Tag 属性
                });
            }
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择要删除的图片");
                return;
            }

            var result = MessageBox.Show("确定要删除选中的图片吗？", "删除确认", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                foreach (ListViewItem item in listView2.SelectedItems)
                {
                    DatImage datImage = item.Tag as DatImage;
                    File.Delete(datImage.FilePath); // 删除文件
                    listView2.Items.Remove(item); // 从ListView中移除
                }
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedItems();
            }
        }

        private void listView1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // 获取选中的项
                ListViewItem clickedItem = listView1.GetItemAt(e.X, e.Y);
                if (clickedItem != null)
                {
                    clickedItem.Selected = true; // 选中行
                }
            }
        }

        private void DeleteSelectedItems()
        {
            if (listView1.SelectedItems.Count == 0)
            {
                return; // 如果没有选中项，不执行删除操作
            }

            var result = MessageBox.Show("确定要删除选中的文件夹吗？", "删除确认", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    string subFolderPath = item.SubItems[1].Text; // 获取次级目录路径
                    string fullFolderPath = Path.Combine(currentPath, subFolderPath); // 构建完整路径

                    Directory.Delete(fullFolderPath, true); // 递归删除文件夹
                    listView1.Items.Remove(item);
                }
            }
        }

        private void 关闭ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 显示帮助信息
            MessageBox.Show(@"请打开您的微信个人文件夹下的FileStorage/MsgAttach目录。
例如，您的个人文件夹为wxid_12345678，请选择wxid_12345678/FileStorage/MsgAttach目录打开。",
                "帮助信息");
        }

        private void 主页ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            // 在默认浏览器中打开URL
            Process.Start("https://www.github.com");
        }
    }
}
