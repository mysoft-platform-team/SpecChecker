using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ClownFish.Base.Xml;

namespace SpecChecker.CoreLibrary.Config
{
    public static class JobManager
    {
        public static readonly string AppVersion
            = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(JobManager).Assembly.Location).FileVersion;


        private static JobOption[] s_jobList;
        private static bool s_inited = false;
        private static readonly object s_lock = new object();

        public static void Init()
        {
            if( s_inited == false ) {
                lock( s_lock ) {
                    if( s_inited == false ) {
                        LoadAllJobs();
                        s_inited = true;
                    }
                }
            }
        }


        private static void LoadAllJobs()
        {
            //string[] files = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "Task-*.xml", SearchOption.TopDirectoryOnly);
            string[] files = ConfigHelper.GetTaskFileNames();

            List <JobOption> list = new List<JobOption>(files.Length);

            foreach( string file in files ) {
                string xml = ConfigHelper.GetFile(file);
                JobOption job = XmlHelper.XmlDeserialize<JobOption>(xml);
                job.TaskFileName = file;

                if( job.Branch != null ) {  // 这二个属性在作业的XML文件中不需要指定
                    job.Branch.Id = job.Id;
                    job.Branch.Name = job.Name;
                }
                list.Add(job);
            }

            s_jobList = (from x in list
                         orderby x.Id
                         select x
                         ).ToArray();

            SetAllDefaultIgnoreRules();
        }

        public static JobOption[] Jobs
        {
            get {
                Init();
                return s_jobList;
            }
        }

        public static JobOption GetJob(int id)
        {
            Init();

            JobOption job = s_jobList.FirstOrDefault(x => x.Id == id);
            if( job == null )
                throw new ArgumentOutOfRangeException("指定的作业ID不存在：" + id.ToString());

            return job;
        }

        public static BranchSettings GetBranch(int id)
        {
            return GetJob(id).Branch;
        }


        private static void SetAllDefaultIgnoreRules()
        {
            // 网站的应用程序中，这个变量就是NULL值
            if( ConfigHelper.AppSettings == null )
                return;

            string defaultIgnoreRules = ConfigHelper.AppSettings["default-IgnoreRules"];
            string defaultIgnoreDbOjects = ConfigHelper.AppSettings["default-IgnoreDbOjects"];

            if( string.IsNullOrEmpty(defaultIgnoreRules) == false ) {

                foreach( var job in s_jobList ) {
                    BranchSettings b = job.Branch;
                    if( string.IsNullOrEmpty(b.IgnoreRules) )
                        b.IgnoreRules = defaultIgnoreRules;
                    else
                        b.IgnoreRules = b.IgnoreRules + ";" + defaultIgnoreRules;
                }
            }

            if( string.IsNullOrEmpty(defaultIgnoreDbOjects) == false ) {

                foreach( var job in s_jobList ) {
                    BranchSettings b = job.Branch;
                    if( string.IsNullOrEmpty(b.IgnoreDbObjects) )
                        b.IgnoreDbObjects = defaultIgnoreDbOjects;
                    else
                        b.IgnoreDbObjects = b.IgnoreDbObjects + ";" + defaultIgnoreDbOjects;
                }
            }


        }

    }
}
