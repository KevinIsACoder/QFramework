//----------------------------------------------------
// Spark: A Framework For Unity
// Copyright © 2014 - 2015 Jay Hu (Q:156809986)
//----------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace SparkEditor
{
	public static class FileUtil
	{
		/// Get md5 checksum of the specified file.
		/// If the specified file is not exists, return null.
		/// 
		public static string GetMD5Hash(string filePath)
		{
			if (!File.Exists(filePath))
				return null;

			return GetMD5Hash(File.ReadAllBytes(filePath));
		}

		public static string GetMD5Hash(byte[] buffer)
		{
			if (buffer == null)
				return null;

			MD5 md5 = new MD5CryptoServiceProvider();
			return BitConverter.ToString(md5.ComputeHash(buffer)).Replace("-", "").ToLower();
		}

		public static void MakeDirs(string path, bool pathIsFile = true)
		{
			if (pathIsFile) {
				path = Path.GetDirectoryName(path);
			}
			if (!Directory.Exists(path)) {
				try {
					Directory.CreateDirectory(path);
				}catch(Exception e) {
					throw new UnityEngine.UnityException("Can't create directories for '" + path + "' (" + e.Message + ")");
				}
			}
		}

		public static byte[] ReadBytes(string filePath)
		{
			if (!File.Exists(filePath))
				return null;

			return File.ReadAllBytes(filePath);
		}

		public static string ReadString(string filePath)
		{
			if (!File.Exists(filePath))
				return null;

			return File.ReadAllText(filePath, Encoding.UTF8);
		}

		public static void WriteBytes(string filePath, byte[] data)
		{
			MakeDirs(filePath);
			File.WriteAllBytes(filePath, data);
		}
		public static void WriteString(string filePath, string content, Encoding encoding)
		{
			MakeDirs(filePath);

			File.WriteAllText(filePath, content.Replace(Environment.NewLine, "\n"), encoding);

			//using (var sw = new StreamWriter(new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite), Encoding.UTF8)) {
			//	sw.Write(content.Replace(Environment.NewLine, "\n"));
			//}
		}

		public static void WriteString(string filePath, string content)
		{
			//var utf8WithoutBOM = new System.Text.UTF8Encoding(false);
			WriteString(filePath, content, new UTF8Encoding(false));
		}

		public static void CopyFile(string from, string to, bool overwrite)
		{
			MakeDirs(to);
			File.Copy(from, to, overwrite);
		}

	}
}
