// SuperController
using System;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MVR.FileManagement;

private string ExtractZipFile(string archiveFilenameIn)
{
	ZipFile zipFile = null;
	string result = null;
	bool flag = false;
	try
	{
		using (FileEntryStream fileEntryStream = FileManager.OpenStream(archiveFilenameIn, restrictPath: true))
		{
			zipFile = new ZipFile(fileEntryStream.Stream);
			string directoryName = FileManager.GetDirectoryName(archiveFilenameIn);
			string fileName = Path.GetFileName(archiveFilenameIn);
			fileName = fileName.Replace(".zip", string.Empty);
			fileName = fileName.Replace(".vac", string.Empty);
			directoryName = directoryName + "/" + fileName;
			foreach (ZipEntry item in zipFile)
			{
				if (!item.IsFile)
				{
					continue;
				}
				string text = item.Name;
				byte[] buffer = new byte[4096];
				Stream inputStream = zipFile.GetInputStream(item);
				string text2 = Path.Combine(directoryName, text);
				string fileName2 = Path.GetFileName(text);
				if (text.EndsWith(".var"))
				{
					text2 = "AddonPackages/" + fileName2;
					string packageUidOrPath = fileName2.Replace(".var", string.Empty);
					if (File.Exists(text2) || FileManager.GetPackage(packageUidOrPath) != null)
					{
						continue;
					}
					flag = true;
				}
				else
				{
					string directoryName2 = Path.GetDirectoryName(text2);
					if (directoryName2.Length > 0)
					{
						FileManager.CreateDirectory(directoryName2);
					}
					if (fileName2 != "meta.json" && (text2.EndsWith(".vac") || text2.EndsWith(".json")))
					{
						result = text2;
					}
				}
				using FileStream destination = File.Create(text2);
				StreamUtils.Copy(inputStream, destination, buffer);
			}
		}
		if (flag)
		{
			FileManager.Refresh();
			return result;
		}
		return result;
	}
	catch (Exception ex)
	{
		Error("Exception during zip file extract of " + archiveFilenameIn + ": " + ex);
		return result;
	}
	finally
	{
		if (zipFile != null)
		{
			zipFile.IsStreamOwner = true;
			zipFile.Close();
		}
	}
}
