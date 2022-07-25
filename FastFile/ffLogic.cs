using Org.BouncyCastle.Utilities.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ffPacker
{
	public class new_ff_file
	{
		public static int fileAmount = 0;
		List<file_tag> Tags = new List<file_tag>();
		public static new_ff_file tagCollection = new new_ff_file();
		public static bool big_endian;
		public static int arraySize, intSize, difference, headerEntry;
		public static void countFiles(string path)
		{
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
			foreach (string _file in files)
			{
				if (_file.Contains(".gsc") ^ _file.Contains(".cfg") ^ _file.Contains(".vision") ^ _file.Contains(".arena") ^ _file.Contains(".atr"))
				{
					fileAmount++;
					string[] splitPath = _file.Remove(0, Program.modFolderPath.Length).Split(new char[] { '\\' });
					string combinedPath = Path.Combine(splitPath);
					tagCollection.Tags.Add(new file_tag(_file, "rawfile", true, combinedPath));
					Program.WriteColoredLine(combinedPath, ConsoleColor.Yellow);
				}
			}
		}
		public static void CompileFinish(string output)
		{
			Console.WriteLine();
			Console.WriteLine("==================================================================");
			Program.WriteColoredLine("Rawfiles found : " + fileAmount, ConsoleColor.Gray);
			Program.WriteColoredLine("Platform : " + Program.compInfo.platform, ConsoleColor.Gray);
			Program.WriteColoredLine("Language : " + Program.compInfo.language, ConsoleColor.Gray);
			Program.WriteColoredLine("Filename : " + Program.compInfo.fileName, ConsoleColor.Gray);
			Program.WriteColoredLine("Game : " + Program.compInfo.game, ConsoleColor.Gray);
			Console.WriteLine("==================================================================");
			Console.WriteLine();
			Program.WriteColoredLine(new DirectoryInfo(Program.modFolderPath).Name + " compiled successfully!", ConsoleColor.Green);
			Program.WriteColoredLine("Output path : " + output, ConsoleColor.Green);
		}
		public void build_zone(byte platform, string fileName, string fileLocation)
		{
			big_endian = platform != 1;
			countFiles(Program.modFolderPath);

			if(fileAmount < 1)
            {
				Program.WriteColoredLine("Cant find any supported files, press any key to exit the application",ConsoleColor.Red);
				Console.ReadKey();
				return;
            }

			switch (platform)
			{
				case (byte)0: //xbox
					arraySize = 40;
					intSize = 34;
					difference = 32;
					headerEntry = 269;
					break;
				case (byte)1: //pc
					arraySize = 48;
					intSize = 36;
					difference = 40;
					headerEntry = 276;
					break;
				default:
					return;
			}
			byte[] array = new byte[arraySize];

			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(array, 0, arraySize);

				_Int32(memoryStream, fileAmount + 1, -1);

				for (int i = 0; i < fileAmount + 1; i++)
					_Int32(memoryStream, intSize, -1);

				List<ZOutputStream> zList = new List<ZOutputStream>();

				foreach (file_tag gsc_info in tagCollection.Tags)
				{
					ZOutputStream zoutputStream = creategsc(memoryStream, gsc_info);
					
					if (zoutputStream is null)
						continue;

					zList.Add(zoutputStream);
					
				}
				_Int32(memoryStream, -1, 0, 0,-1);
				_String(memoryStream, fileName, fileName.Length);
				_Byte(memoryStream, 0, 0);
				long savedPosition = memoryStream.Position;
				memoryStream.Position = 0;
				_Int32(memoryStream, Convert.ToInt32(savedPosition) - difference); 
				memoryStream.Position = 8;
				_Int32(memoryStream, 32);
				memoryStream.Position = 20;
				_Int32(memoryStream, Convert.ToInt32(savedPosition));
				memoryStream.Position = savedPosition;
				build_ff(platform, fileName, 1, memoryStream, fileLocation, zList);
			}
		}
		private ZOutputStream creategsc(MemoryStream zone, file_tag gsc_info)
		{
			using (Stream stream = new FileStream(gsc_info.file_path, FileMode.Open, FileAccess.Read))
			{
				_Int32(zone, -1);
				long position = zone.Position;
				_Int32(zone, 0, Convert.ToInt32(stream.Length), -1);
				string text = gsc_info.packaged_path.Replace('\\', '/').Replace("\n", string.Empty);
				_String(zone, text, text.Length);
				_Byte(zone, 0);

				if (gsc_info.compress)
				{
					ZOutputStream zoutputStream = new ZOutputStream(zone, 9);
					
					byte[] buffer = new byte[16384];
					zoutputStream.FlushMode = 2;
					int num = Convert.ToInt32(zone.Position);
					int count;
					while ((count = stream.Read(buffer, 0, 16384)) > 0)
					{
						zoutputStream.Write(buffer, 0, count);
						zone.Flush();
						zoutputStream.Flush();
					}
					zoutputStream.Finish();
					int num2 = Convert.ToInt32(zone.Position);
					zone.Position = position;
					_Int32(zone, num2 - num);
					zone.Position = (long)num2;
					return zoutputStream;
					
				}
				byte[] array = new byte[16384];
				int count2;
				while ((count2 = stream.Read(array, 0, array.Length)) != 0)
				{
					zone.Write(array, 0, count2);
				}
				stream.Flush();
			}
			return null;
		}
		public void build_ff(byte platform, string filename, byte can_update, MemoryStream zone, string file_name, List<ZOutputStream> gsc_streams)
		{
			FileStream fileStream = new FileStream(file_name, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
			_String(fileStream, "IWff0100", 8);
			_Int32(fileStream, headerEntry);
			_Byte(fileStream, can_update);
			_Date(fileStream, DateTime.Now.ToFileTime(), big_endian);

			if (platform != 1)
			{
				_Int32(fileStream, Program.languageValue, 0, 0, 0);
			}

			_String(fileStream, "IWffs100", 8);
			byte[] array = new byte[292];
			fileStream.Write(array, 0, array.Length);
			_String(fileStream, filename, filename.Length);
			byte[] array2 = new byte[36 - filename.Length];
			fileStream.Write(array2, 0, array2.Length);
			byte[] array3 = new byte[16048];
			fileStream.Write(array3, 0, array3.Length);
			zone.Position = 0L;
			ZOutputStream zoutputStream = new ZOutputStream(fileStream, 9);
			byte[] buffer = new byte[16384];
			int count;
			while ((count = zone.Read(buffer, 0, 16384)) > 0)
			{
				zoutputStream.Write(buffer, 0, count);
			}
			zoutputStream.Flush();
			foreach (ZOutputStream zoutputStream2 in gsc_streams)
			{
				zoutputStream2.Close();
			}
			zoutputStream.Close();

			using (FileStream fileStream2 = new FileStream(file_name, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
			{
				if (platform == 0)
				{
					fileStream2.Position = 29;
					_Int32(fileStream2, Convert.ToInt32(fileStream2.Length));
					_Int32(fileStream2, Convert.ToInt32(fileStream2.Length));
					fileStream2.Position = fileStream2.Length;
					while (fileStream2.Length % 8192 != 37)
					{
						fileStream2.WriteByte(0);
					}
				}
				if (platform == 1)
				{
					fileStream2.Position = fileStream2.Length;
					while (fileStream2.Length % 8192 != 21)
					{
						fileStream2.WriteByte(0);
					}
				}
				CompileFinish(Program.modOutputPath);
			}
		}
		private static void _String(Stream stream, string value, int len)
		{
			byte[] array = new byte[len + 1];
			for (int i = 0; i < len; i++)
			{
				array[i] = (byte)value[i];
			}
			stream.Write(array, 0, len);
		}
		private static void _Int32(Stream stream, params int[] values)
		{
			foreach (int value in values)
			{
				if (!big_endian)
				{
					stream.Write(BitConverter.GetBytes(value), 0, 4);
					return;
				}
				byte[] bytes = BitConverter.GetBytes(value);
				byte[] array = new byte[4];
				for (int i = 0; i < 4; i++)
				{
					array[3 - i] = bytes[i];
				}
				stream.Write(array, 0, 4);
			}
		}
		private static void _Byte(Stream stream, params byte[] values)
		{
			foreach (byte value in values)
			{
				stream.WriteByte(value);
			}
		}
		private static void _Date(Stream stream, long time, bool big_endian)
		{
			if (big_endian)
			{
				byte[] bytes = BitConverter.GetBytes(time);
				byte[] array = new byte[4];
				byte[] array2 = new byte[4];
				for (int i = 0; i < 4; i++)
				{
					array[3 - i] = bytes[i];
					array2[3 - i] = bytes[i + 4];
				}
				stream.Write(array2, 0, 4);
				stream.Write(array, 0, 4);
				return;
			}
			byte[] bytes2 = BitConverter.GetBytes(time);
			byte[] array3 = new byte[4];
			byte[] array4 = new byte[4];
			for (int j = 0; j < 4; j++)
			{
				array3[j] = bytes2[j];
				array4[j] = bytes2[j + 4];
			}
			stream.Write(array4, 0, 4);
			stream.Write(array3, 0, 4);
		}
	}
}
