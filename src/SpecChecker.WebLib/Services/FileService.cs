using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClownFish.Web;
using SpecChecker.CoreLibrary.Config;

namespace SpecChecker.WebLib.Services
{
    public class FileService : BaseController
    {
        public string GetFile(string filename)
        {
            if( string.IsNullOrEmpty(filename) )
                throw new ArgumentNullException(nameof(filename));
            if( filename.IndexOf('/') >= 0 || filename.IndexOf('\\') >= 0 )
                throw new ArgumentException("文件参数不允许包含路径。");


            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App_Data\Config\" + filename);
            return File.ReadAllText(filePath, Encoding.UTF8);
        }


        public string GetTaskFile(int id)
        {
            var job = JobManager.GetJob(id);

            this.WriteHeader("x-filename", System.Web.HttpUtility.UrlEncode(Path.GetFileName(job.TaskFileName)));
            return File.ReadAllText(job.TaskFileName, Encoding.UTF8);
        }


        [Action(OutFormat = ClownFish.Base.Http.SerializeFormat.Form)]
        public object GetAppSettings()
        {
            // 仅返回给客户端使用的配置参数

            string key1 = "default-IgnoreRules";
            string key2 = "default-IgnoreDbOjects";

            return new Dictionary<string, string> {
                { key1, ConfigurationManager.AppSettings[key1] },
                { key2, ConfigurationManager.AppSettings[key2] }
            };
        }


        public string GetAppVersion()
        {
            return SpecChecker.CoreLibrary.Config.JobManager.AppVersion;
        }


        public IActionResult GetClientZip()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Client\SpecChecker.ConsoleApp.zip");
            if( File.Exists(filePath) == false )
                throw new FileNotFoundException("找不到客户端压缩包文件：" + filePath);

            return new ClownFish.Web.FileDownloadResult(filePath, null);
        }
    }
}
