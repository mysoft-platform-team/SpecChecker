using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecChecker.CoreLibrary.Common
{
	public static class ZipHelper
	{
		public static void CreateZipFileFromText(string zipPath, string text)
		{
			if( text == null )
				throw new ArgumentNullException("text");

			byte[] bb = Encoding.UTF8.GetBytes(text);
			string name = "file1.txt";

			using( FileStream file = new FileStream(zipPath, FileMode.Create) ) {
				using( ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Create, true, Encoding.UTF8) ) {

					var entry = zip.CreateEntry(name, CompressionLevel.Optimal);
					using( BinaryWriter writer = new BinaryWriter(entry.Open()) ) {
						writer.Write(bb);
					}
				}
			}
		}


		public static string ReadTextFromZipFile(string zipPath)
		{
			byte[] bb = ReadZipFile(zipPath, null);

			using( MemoryStream ms = new MemoryStream(bb) ) {
				using( StreamReader reader = new StreamReader(ms, Encoding.UTF8) ) {
					return reader.ReadToEnd();
				}
			}

			// 下面这种读取方法会把文件的BOM也读出来。
			//return Encoding.UTF8.GetString(bb);
		}

		public static void CreateZipFile(string zipPath, params string[] srcFiles)
		{
			if( string.IsNullOrEmpty(zipPath) )
				throw new ArgumentNullException("zipPath");
			if( srcFiles == null || srcFiles.Length == 0 )
				throw new ArgumentNullException("srcFiles");

			using( FileStream file = new FileStream(zipPath, FileMode.Create) ) {
				using( ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Create, true, Encoding.UTF8) ) {

					foreach( string srcFile in srcFiles ) {
						byte[] bb = File.ReadAllBytes(srcFile);
						string name = Path.GetFileName(srcFile);

						var entry = zip.CreateEntry(name, CompressionLevel.Optimal);

						using( BinaryWriter writer = new BinaryWriter(entry.Open()) ) {
							writer.Write(bb);
						}
					}
				}
			}
		}


		public static byte[] ReadZipFile(string zipPath, string name)
		{
			if( string.IsNullOrEmpty(zipPath) )
				throw new ArgumentNullException("zipPath");

			using( FileStream file = new FileStream(zipPath, FileMode.Open) ) {
				using( ZipArchive zip = new ZipArchive(file, ZipArchiveMode.Read, true, Encoding.UTF8) ) {
					ZipArchiveEntry entry = string.IsNullOrEmpty(name) && zip.Entries.Count > 0
						? zip.Entries[0]
						: zip.Entries.FirstOrDefault(x => x.Name == name);

					if( entry == null )
						return null;

					using( BinaryReader reader = new BinaryReader(entry.Open()) ) {
						return reader.ReadBytes((int)entry.Length);
					}
				}
			}
		}


		public static void ExtractFiles(string zipPath, string extractPath)
		{
			if( string.IsNullOrEmpty(zipPath) )
				throw new ArgumentNullException("zipPath");
			if( string.IsNullOrEmpty(extractPath) )
				throw new ArgumentNullException("extractPath");

			using( ZipArchive archive = ZipFile.OpenRead(zipPath) ) {
				foreach( ZipArchiveEntry entry in archive.Entries ) {
					string targetFile = Path.Combine(extractPath, entry.FullName);
					if( File.Exists(targetFile) )
						File.Delete(targetFile);

					entry.ExtractToFile(targetFile);
				}
			}
		}



	}

}
