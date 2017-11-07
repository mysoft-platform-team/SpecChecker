using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Base;
using ClownFish.Base.WebClient;

namespace SpecChecker.CoreLibrary.Config
{
	public static class ConfigHelper
	{
        // 将客户端下载的配置文件保存到内存中
        private static Dictionary<string, string> s_configFileTextDict = null;

        internal static NameValueCollection AppSettings { get; private set; }


        /// <summary>
        /// 客户端工具的初始化，包含以下操作：
        /// 1、下载AppSettings配置
        /// 2、下载配置文件及任务文件
        /// </summary>
        public static void ClientInit()
        {
            // 下载用于客户端的配置参数
            GetAppSettings();

            // 下载配置文件
            DownloadConfigFiles();
        }


        /// <summary>
        /// 下载配置文件（仅供客户端程序调用）
        /// </summary>
        private static void DownloadConfigFiles()
        {
            // 下载配置文件
            s_configFileTextDict = new Dictionary<string, string>(32, StringComparer.OrdinalIgnoreCase);
            int[] idList = GetBranchIdList();

            // 根据app.config中的配置，下载作业文件
            foreach( int id in idList )
                DownloadTaskFile(id);

            // 下载其它的配置文件
            DownloadFile("SpecChecker.CodeRule.config");
            DownloadFile("SpecChecker.IssueCategory.config");
        }

        private static void GetAppSettings()
        {
            string website = ConfigurationManager.AppSettings["ServiceWebsite"];
            HttpOption option = new HttpOption {
                Method = "GET",
                Url = website.TrimEnd('/') + "/ajax/scan/File/GetAppSettings.ppx"
            };
            string text = option.GetResult();

            AppSettings = System.Web.HttpUtility.ParseQueryString(text);
        }


        

        private static int[] GetBranchIdList()
        {
            // 读取当前要处理的分支ID
            string configValue = ConfigurationManager.AppSettings["branch-id"];
            if( string.IsNullOrEmpty(configValue) )
                throw new ConfigurationErrorsException("branch-id 配置项不能为空。");

            // 将配置的字符串转换成数字ID列表
            try {
                return (from x in configValue.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                          let id = int.Parse(x)
                          select id).ToArray();
            }
            catch( Exception ex ) {
                throw new ConfigurationErrorsException("branch-id 配置内容无效。", ex);
            }
        }

        private static void DownloadFile(string filename)
        {
            string website = ConfigurationManager.AppSettings["ServiceWebsite"];

            HttpOption option = new HttpOption {
                Method = "POST",
                Url = website.TrimEnd('/') + "/ajax/scan/File/GetFile.ppx",
                Data = new { filename = filename }
            };
            string text = option.GetResult();

            s_configFileTextDict[filename] = text;
        }

        private static void DownloadTaskFile(int id)
        {
            string filename = null;
            string website = ConfigurationManager.AppSettings["ServiceWebsite"];

            HttpOption option = new HttpOption {
                Method = "POST",
                Url = website.TrimEnd('/') + "/ajax/scan/File/GetTaskFile.ppx",
                Data = new { id = id }
            };
            option.ReadResponseAction = x => filename = System.Web.HttpUtility.UrlDecode(x.GetResponseHeader("x-filename"));
            string text = option.GetResult();

            s_configFileTextDict[filename] = text;
        }


        public static string[] GetTaskFileNames()
        {
            // 文件名格式："Task-*.xml"

            // 客户端模式
            if( s_configFileTextDict != null ) {
                return (from x in s_configFileTextDict
                        where x.Key.StartsWithIgnoreCase("Task-") && x.Key.EndsWithIgnoreCase(".xml")
                        select x.Key
                        ).ToArray();
            }
            else {
                // 站点模式
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App_Data\Config");
                return Directory.GetFiles(path, "Task-*.xml", SearchOption.TopDirectoryOnly);
            }
        }


        public static string GetFile(string filename)
        {
            // 客户端模式，文件从内存变量中加载
            if( s_configFileTextDict != null )
                // 如果指定的文件名无效，将会出现异常
                return s_configFileTextDict[filename];


            // 查找网站下的config目录
            string path = Path.IsPathRooted(filename)
                            ? filename
                            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App_Data\config\" + filename);

            if( File.Exists(path) )
                return File.ReadAllText(path, Encoding.UTF8);

            throw new FileNotFoundException($"没有找到 {filename} 文件。");
        }


        

    }
}
