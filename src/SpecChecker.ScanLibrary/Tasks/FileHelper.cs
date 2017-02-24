using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.ScanLibrary.Tasks
{
	internal class FileHelper
	{
		public void ClearBinObjFiles(string rootPath)
		{
			string[] binDirectories = Directory.GetDirectories(rootPath, "bin", SearchOption.AllDirectories);
			foreach( string bin in binDirectories )
				ClearDirectory(bin);

			string[] objDirectories = Directory.GetDirectories(rootPath, "obj", SearchOption.AllDirectories);
			foreach( string obj in objDirectories )
				ClearDirectory(obj);
		}


		public void ClearFiles(string path)
		{
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

			foreach( string file in files ) {
				try {
					File.Delete(file);
				}
				catch {// 有可能是文件只读，导致删除失败
				}
			}
		}


		/// <summary>
		/// 删除指定目录下的所有文件及子目录
		/// </summary>
		/// <param name="path"></param>
		internal void ClearDirectory(string path)
		{
			// 有可能在递归删除中被删除了
			if( Directory.Exists(path) == false )
				return;

			ClearFiles(path);

			string[] dirs = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);
			foreach( string dir in dirs ) {
				try {
					Directory.Delete(dir);
				}
				catch { }
			}
		}
	}
}
