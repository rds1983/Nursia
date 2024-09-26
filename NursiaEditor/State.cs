using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using NursiaEditor.Utility;

namespace NursiaEditor
{
	public class State
	{
		public const string StateFileName = "NursiaEditor.config";

		public static string StateFilePath
		{
			get
			{
				var result = Path.Combine(PathUtils.ExecutingAssemblyDirectory, StateFileName);
				return result;
			}
		}

		public Point Size { get; set; }
		public float TopSplitterPosition { get; set; }
		public float LeftSplitterPosition { get; set; }
		public string EditedFile { get; set; }
		public bool ShowGrid { get; set; }
		public bool DrawBoundingBoxes { get; set; }
		public bool DrawLightViewFrustum { get; set; }
		public bool DrawShadowMap { get; set; }

		public State()
		{
		}

		public void Save()
		{
			using (var fileStream = File.Create(StateFilePath))
			{
				var xmlWriter = new XmlTextWriter(fileStream, Encoding.UTF8)
				{
					Formatting = Formatting.Indented
				};
				var serializer = new XmlSerializer(typeof(State));
				serializer.Serialize(xmlWriter, this);
			}
		}

		public static State Load()
		{
			if (!File.Exists(StateFilePath))
			{
				return null;
			}

			State state;
			using (var stream = new StreamReader(StateFilePath))
			{
				var serializer = new XmlSerializer(typeof(State));
				state = (State)serializer.Deserialize(stream);
			}

			return state;
		}

		public override string ToString()
		{
			return string.Format("Size = {0}\n" +
								 "TopSplitter = {1:0.##}\n" +
								 "LeftSplitter= {2:0.##}\n" +
								 "EditedFile = {3}",
				Size,
				TopSplitterPosition,
				LeftSplitterPosition,
				EditedFile);
		}
	}
}