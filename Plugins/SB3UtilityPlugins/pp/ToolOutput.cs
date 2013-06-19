﻿using System.IO;

namespace SB3Utility
{
	public class ToolOutputParser : IWriteFile
	{
		public byte[] contents;

		public ToolOutputParser(Stream stream, string name)
			: this(stream)
		{
			this.Name = name;
		}

		public ToolOutputParser(Stream stream)
		{
			using (BinaryReader reader = new BinaryReader(stream))
			{
				contents = reader.ReadToEnd();
			}
		}

		public string Name { get; set; }

		public void WriteTo(Stream stream)
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write(contents);
		}
	}

	public static partial class Plugins
	{
		[Plugin]
		public static ToolOutputParser OpenToolOutput([DefaultVar]ppParser parser, string name)
		{
			for (int i = 0; i < parser.Subfiles.Count; i++)
			{
				if (parser.Subfiles[i].Name == name)
				{
					IReadFile subfile = parser.Subfiles[i] as IReadFile;
					if (subfile != null)
					{
						return new ToolOutputParser(subfile.CreateReadStream(), subfile.Name);
					}

					break;
				}
			}
			return null;
		}
	}
}
