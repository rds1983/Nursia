using System;
using System.Collections.Generic;
using System.Text;

namespace SampleScene
{
	public class GenerationConfig
	{
		private static GenerationConfig _instance;

		public int WorldSize { get; set; }
		public int HeightMapVariability { get; set; }
		public bool SurroundedByWater { get; set; }
		public bool Smooth { get; set; }

		public static GenerationConfig Instance
		{
			get
			{
				return _instance;
			}

			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}

				_instance = value;
			}
		}

		static GenerationConfig()
		{
			Instance = new GenerationConfig();
		}

		private GenerationConfig()
		{
			WorldSize = 1024;
			HeightMapVariability = 5;
			Smooth = true;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			sb.Append("WorldSize=" + WorldSize + ",\n");
			sb.Append("HeightMapVariability=" + HeightMapVariability + ",\n");
			sb.Append("SurroundedByWater=" + SurroundedByWater + ",\n");
			sb.Append("Smooth=" + Smooth + ",\n");

			return sb.ToString();
		}
	}
}