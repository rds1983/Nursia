using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using static glTFLoader.Schema.Accessor;

namespace Nursia.Utilities
{
	internal static class Serialization
	{
		private static readonly int[] ComponentsCount = new[]
		{
			1,
			2,
			3,
			4,
			4,
			9,
			16
		};

		private static readonly int[] ComponentSizes = new[]
		{
			sizeof(sbyte),
			sizeof(byte),
			sizeof(short),
			sizeof(ushort),
			0,	// There's no such component
			sizeof(uint),
			sizeof(float)
		};

		public static float ToFloat(this JToken data)
		{
			return Convert.ToSingle(data, CultureInfo.InvariantCulture);
		}

		public static Vector3 ToVector3(this JToken data)
		{
			var floats = (JArray)data;
			return new Vector3(floats[0].ToFloat(),
				floats[1].ToFloat(),
				floats[2].ToFloat());
		}

		public static Vector4 ToVector4(this JToken data, float defW = 0.0f)
		{
			var floats = (JArray)data;
			var result = new Vector4();
			result.X = floats[0].ToFloat();
			result.Y = floats[1].ToFloat();
			result.Z = floats[2].ToFloat();

			if (floats.Count > 3)
			{
				result.W = floats[3].ToFloat();
			}
			else
			{
				result.W = defW;
			}

			return result;
		}

		public static string GetId(this JObject data)
		{
			var result = string.Empty;

			JToken obj;
			if (data.TryGetValue("id", out obj) && obj != null)
			{
				result = obj.ToString();
			}

			return result;
		}

		public static JToken EnsureObject(this JObject data, string key)
		{
			JToken obj;
			if (!data.TryGetValue(key, out obj))
			{
				throw new Exception(string.Format("Mandatory field '{0}' missing.", key));
			}

			return obj;
		}

		public static string EnsureString(this JObject data, string key)
		{
			var obj = EnsureObject(data, key);

			return obj != null ? obj.ToString() : string.Empty;
		}

		public static T ParseEnum<T>(this JToken data)
		{
			return (T)Enum.Parse(typeof(T), data.ToString());
		}

		public static int GetComponentCount(this TypeEnum type) => ComponentsCount[(int)type];
		public static int GetComponentSize(this ComponentTypeEnum type) => ComponentSizes[(int)type - 5120];
	}
}