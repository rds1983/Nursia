using System;
using System.IO;

namespace Nursia.ModelImporter
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Usage: ModelImporter <inputFile> <outputFile>");
			}

			var inputFile = args[0];
			var outputFile = args[1];

			var importer = new Importer();
			var root = importer.Import(inputFile);

			var exporter = new Exporter();
			string output = exporter.Export(root);

			File.WriteAllText(outputFile, output);
		}
	}
}
