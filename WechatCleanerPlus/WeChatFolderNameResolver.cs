using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace WechatCleanerPlus
{
    internal class WeChatFolderNameResolver
    {
        public void ResolveFolderNames(ref List<string[]> listRows)
        {
            // 第一步：生成SaveDirList.txt
            GenerateSaveDirList(listRows);

            // 第二步：执行GetNickNames.exe
            ExecuteGetNickNames();

            // 第三步：读取ContactNickNameList.txt并更新listRows
            UpdateListRowsWithNickNames(ref listRows);
        }

        private void GenerateSaveDirList(List<string[]> listRows)
        {
            using (StreamWriter file = new StreamWriter("SaveDirList.txt"))
            {
                foreach (var row in listRows)
                {
                    file.WriteLine(row[1]); // 假设第二列包含我们需要的文件夹名称
                }
            }
        }

        private void ExecuteGetNickNames()
        {
            Process process = new Process();
            process.StartInfo.FileName = "GetNickNames.exe";
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory(); // 确保exe在当前目录
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
        }

        private void UpdateListRowsWithNickNames(ref List<string[]> listRows)
        {
            string[] nickNames = File.ReadAllLines("ContactNickNameList.txt");
            for (int i = 0; i < listRows.Count && i < nickNames.Length; i++)
            {
                listRows[i][0] = nickNames[i]; // 更新第一列为NickName
            }
        }
    }
}
