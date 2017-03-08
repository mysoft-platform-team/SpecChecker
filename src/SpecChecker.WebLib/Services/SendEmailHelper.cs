//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Mail;
//using System.Text;
//using System.Threading.Tasks;
//using ClownFish.Base;
//using ClownFish.Log.Model;
//using SpecChecker.CoreLibrary;
//using SpecChecker.CoreLibrary.Config;
//using SpecChecker.WebLib.ViewModel;

//namespace SpecChecker.WebLib.Services
//{
//    public class SendEmailHelper
//    {
//		public static void Send(DateTime? day, int branchId, Uri requestUri)
//		{
//			DateTime today = day.HasValue ? day.Value.Date : DateTime.Today;

//			var branch = BranchManager.ConfingInstance.Branchs.Find(t => t.Id == branchId);
//			if( branch == null || string.IsNullOrEmpty(branch.ExceptionAlertEmail) )
//				return;

//			// 加载各小组的分类汇总数据
//			DailySummaryHelper helper = new DailySummaryHelper();

//			QaReportTableConvert convert = new QaReportTableConvert();
//			convert.TodaySummary = helper.LoadData(today);
//			convert.LastdaySummary = helper.LoadData(today.AddDays(-1d));

//			var qaReportTable = convert.ToTableData();
//			if( qaReportTable == null )
//				return;

//			StringBuilder mailMessage = new StringBuilder();

			

			

//			foreach( var row in qaReportTable.Rows ) {
//				//种类
//				//row.ScanKind
//				int i = 0;
//				foreach( var cell in row.Cells ) {
//					if( QaReportTableConvert.GroupNames[i] == branch.Name && cell.Direction == 1 ) {
//						mailMessage.AppendLine($"{row.ScanKind}问题上升至{cell.Value}");
//					}
//					i++;
//				}
//			}

//			if( mailMessage.Length > 0 ) {
//				//邮箱
//				var emails = branch.ExceptionAlertEmail.Split(';');
//				//mailMessage.AppendLine("http://spec.mysoft.com.cn:55768/Report/" + today.ToDateString() + ".phtml");
//				mailMessage.AppendFormat("{0}://{1}:{2}/Report/{3}.phtml", 
//										requestUri.Scheme, requestUri.Host, requestUri.Port, 
//										today.ToDateString());

//				SendEmail(emails, 
//					$"{branch.Name}小组代码检查有新增问题，请及时关注！{today.ToDateString()}", 
//					mailMessage.ToString() );
//			}
//		}

//        private static void SendEmail(string[] to, string subject, string body)
//        {
//            try
//            {
//                if (to == null || to.Length == 0)
//                    throw new ArgumentNullException("to");


//                MailMessage mail = new MailMessage();
//                foreach (string address in to)
//                    mail.To.Add(new MailAddress(address));

//                mail.Subject = subject;
//                mail.Body = body;
//                mail.BodyEncoding = System.Text.Encoding.UTF8;
//                mail.SubjectEncoding = System.Text.Encoding.UTF8;
//                mail.IsBodyHtml = false;


//                SmtpClient smtp = new SmtpClient();
//                string from = (smtp.Credentials as System.Net.NetworkCredential).UserName;
//                //smtp.Credentials = System.Net.NetworkCredential;
//                mail.From = new MailAddress(from);
//                smtp.EnableSsl = true;
//                smtp.Send(mail);


//                //SmtpClient client = new SmtpClient();
//                //client.Credentials = new System.Net.NetworkCredential
//                //    ("ptgx@mysoft.com.cn", "4622Dell");

//                //client.Port = 587;//or use 587            
//                //client.Host = "smtp.exmail.qq.com";
//                //client.EnableSsl = true;
//                //client.Send(mail);

//            }
//            catch (Exception ex)
//            {
//                ExceptionInfo exceptionInfo = ExceptionInfo.Create(ex);
//                ClownFish.Log.LogHelper.SyncWrite(exceptionInfo);
//            }
//        }
//    }
//}
