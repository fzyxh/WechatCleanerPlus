using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WechatCleanerPlus
{
    internal class ListViewItemComparer : IComparer
    {
        private int col;
        private SortOrder order;

        public ListViewItemComparer()
        {
            col = 0;
            order = SortOrder.Ascending;
        }

        public ListViewItemComparer(int column, SortOrder order)
        {
            col = column;
            this.order = order;
        }

        public int Compare(object x, object y)
        {
            int returnVal = -1;
            if (col == 2) // 数值列（大小）
            {
                // 解析并比较数值大小
                double xSize = ConvertSizeToBytes(((ListViewItem)x).SubItems[col].Text);
                double ySize = ConvertSizeToBytes(((ListViewItem)y).SubItems[col].Text);
                Debug.WriteLine($"xSize: {xSize.ToString()}, ySize: {ySize.ToString()}");

                returnVal = xSize.CompareTo(ySize);
            }
            else // 字符串列（目录名称）
            {
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text);
            }

            if (order == SortOrder.Descending)
                returnVal *= -1;

            return returnVal;
        }

        private double ConvertSizeToBytes(string sizeStr)
        {
            string[] sizeParts = sizeStr.Split(' ');
            if (sizeParts.Length != 2) return 0;

            double size = double.Parse(sizeParts[0]);
            switch (sizeParts[1].ToLower())
            {
                case "kb":
                    return size * 1024;
                case "mb":
                    return size * 1024 * 1024;
                case "gb":
                    return size * 1024 * 1024 * 1024;
                case "tb":
                    return size * 1024 * 1024 * 1024 * 1024;
                default: // 假设为字节
                    return size;
            }
        }
    }
}
