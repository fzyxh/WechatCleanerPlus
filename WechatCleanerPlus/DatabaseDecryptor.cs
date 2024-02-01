using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;

namespace WechatCleanerPlus
{
    internal class DatabaseDecryptor
    {
        private const int KeySize = 32;
        private const int DefaultIter = 64000;
        private const int DefaultPageSize = 4096; // 4048 data + 16 IV + 20 HMAC + 12
        private static readonly byte[] SqliteFileHeader = Encoding.ASCII.GetBytes("SQLite format 3\0");

        public static string GetWeChatDatabaseKey(string weChatId)
        {
            try
            {
                // 创建一个新的进程来执行GetWeChatAesKey.exe程序
                Process process = new Process();
                process.StartInfo.FileName = "GetWeChatAesKey.exe";
                process.StartInfo.Arguments = $"-i {weChatId}";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                process.WaitForExit();

                // 读取程序的标准输出，即密钥
                string output = process.StandardOutput.ReadToEnd().Trim();
                process.Close();

                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生错误：" + ex.Message);
                return null;
            }
        }

        public static void DecryptDatabase(string databasePath, string databaseKey)
        {
            try
            {
                // 确保WCPCache目录存在
                string targetDir = Path.Combine(Directory.GetCurrentDirectory(), "WCPCache");
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // 构建目标数据库路径
                string targetDbPath = Path.Combine(targetDir, Path.GetFileName(databasePath));

                // 如果目标数据库文件已存在，则先删除
                if (File.Exists(targetDbPath))
                {
                    File.Delete(targetDbPath);
                }

                // 复制数据库文件到WCPCache目录，如果目标文件已存在，则覆盖它
                File.Copy(databasePath, targetDbPath, true);

                // 构建完整的命令行参数
                string arguments = $"-p \"{targetDbPath}\" -k {databaseKey}";

                // MessageBox.Show($"尝试解密：{arguments}", "解密数据库", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // 初始化新的进程
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "CrackWeChatDB.exe",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                // 启动进程
                using (Process process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        // 等待外部程序完成执行
                        process.WaitForExit();

                        // 检查解密后的数据库文件是否存在
                        string decryptedDbPath = Path.Combine(targetDir, $"{Path.GetFileNameWithoutExtension(targetDbPath)}.dec.db");
                        if (File.Exists(decryptedDbPath))
                        {
                            Console.WriteLine("数据库解密成功！");
                        }
                        else
                        {
                            Console.WriteLine("数据库解密失败或未生成文件。");
                        }
                    }
                    else
                    {
                        Console.WriteLine("无法启动解密进程。");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误：{ex.Message}");
            }
        }
    }
}
