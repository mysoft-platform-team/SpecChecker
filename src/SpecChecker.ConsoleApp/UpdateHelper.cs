﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base.WebClient;
using SpecChecker.CoreLibrary.Common;

namespace SpecChecker.ConsoleApp
{
    /// <summary>
    /// 用于标记因为需要更新而退出程序的异常类型
    /// </summary>
    internal class AutoUpdateExitException : Exception
    {
    }

    internal static class UpdateHelper
    {
        private static readonly string s_deleteFlagFile 
                                        = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_temppath.txt");


        public static void CheckUpdate()
        {
            // 判断版本是否最新
            string currentVersion = SpecChecker.CoreLibrary.Config.JobManager.AppVersion;
            string serverVersion = GetServerAppVersion();
            if( currentVersion == serverVersion )
                return;


            // 下载新版本的客户端压缩包
            byte[] fileBuffer = GetClientZip();
            string zipFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".zip");
            File.WriteAllBytes(zipFile, fileBuffer);


            // 创建用于释放文件的临时目录
            string tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempPath);
            ZipHelper.ExtractFiles(zipFile, tempPath);

            // 记录要删除的临时文件
            string deleteMessage = zipFile + "\r\n" + tempPath;
            File.WriteAllText(s_deleteFlagFile, deleteMessage, Encoding.UTF8);


            // 启动临时目录下的程序完成更新文件的动作
            Process exe = new Process();
            exe.StartInfo.FileName = Path.Combine(tempPath, "SpecChecker.ConsoleApp.exe");
            exe.StartInfo.Arguments = string.Format(" \"/copy:{0}\"", AppDomain.CurrentDomain.BaseDirectory);
            exe.StartInfo.CreateNoWindow = true;
            exe.StartInfo.WorkingDirectory = tempPath;
            exe.Start();

            // 退出当前程序
            throw new AutoUpdateExitException();
        }


        private static string GetServerAppVersion()
        {
            string website = ConfigurationManager.AppSettings["ServiceWebsite"];
            HttpOption option = new HttpOption {
                Method = "GET",
                Url = website.TrimEnd('/') + "/ajax/scan/File/GetAppVersion.ppx"
            };
            return option.GetResult();
        }

        private static byte[] GetClientZip()
        {
            string website = ConfigurationManager.AppSettings["ServiceWebsite"];
            HttpOption option = new HttpOption {
                Method = "GET",
                Url = website.TrimEnd('/') + "/ajax/scan/File/GetClientZip.ppx"
            };
            return option.GetResult<byte[]>();
        }


        public static void CopyFileAndRestart(string destPath)
        {
            // 等待原进程退出
            System.Threading.Thread.Sleep(3000);

            string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.*");

            foreach(string src in files ) {
                string dest = Path.Combine(destPath, Path.GetFileName(src));
                File.Copy(src, dest, true);
            }


            // 重新启动程序
            Process exe = new Process();
            exe.StartInfo.FileName = Path.Combine(destPath, "SpecChecker.ConsoleApp.exe");
            exe.StartInfo.WorkingDirectory = destPath;
            exe.Start();
        }


        /// <summary>
        /// 尝试删除上一次自动更新遗留的临时文件
        /// </summary>
        public static void DeleteTempDirectory()
        {
            if( File.Exists(s_deleteFlagFile) == false )
                return;

            // 删除目录前等待文件句柄全部关闭
            System.Threading.Thread.Sleep(3000);

            
            try {
                string[] paths = File.ReadAllLines(s_deleteFlagFile, Encoding.UTF8);

                foreach(string path in paths ) {
                    if( File.Exists(path) )
                        File.Delete(path);
                    else {
                        if( Directory.Exists(path))
                            Directory.Delete(path, true);
                    }                       
                }                
            }
            catch { // 临时目录如果删除失败就只能忽略了
            }

            try {
                File.Delete(s_deleteFlagFile);
            }
            catch { // 临时文件如果删除失败就只能忽略了
            }
        }

    }
}
