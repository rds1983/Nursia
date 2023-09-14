using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace Nursia.Utilities
{
	internal static class JsonExtensions
	{
		public static JToken EnsureJToken(this JObject obj, string fieldName)
		{
			var token = obj[fieldName];
			if (token == null)
			{
				RaiseError($"Could not find mandatory '{fieldName}' field.");
			}

			return token;
		}

		public static T JConvertT<T>(this JToken token) where T : JToken
		{
			var asT = token as T;
			if (asT == null)
			{
				RaiseError($"Could not cast '{token}' to '{typeof(T).Name}'.");
			}

			return asT;
		}

		public static T EnsureT<T>(this JObject obj, string fieldName) where T : JToken
		{
			var token = obj.EnsureJToken(fieldName);
			return JConvertT<T>(token);
		}

		public static JArray EnsureJArray(this JObject obj, string fieldName)
		{
			return EnsureT<JArray>(obj, fieldName);
		}

		public static JObject EnsureJObject(this JObject obj, string fieldName)
		{
			return EnsureT<JObject>(obj, fieldName);
		}

		public static string EnsureString(this JObject obj, string fieldName)
		{
			var token = obj.EnsureJToken(fieldName);
			return token.ToString();
		}

		public static T ToEnum<T>(this string value)
		{
			return (T)Enum.Parse(typeof(T), value);
		}

		public static int ToInt(this JToken value)
		{
			return value.ToString().ToInt();
		}

		public static int ToInt(this string value)
		{
			int result;
			if (!int.TryParse(value, out result))
			{
				RaiseError($"Can't parse '{value}' as int value.");
			}

			return result;
		}

		public static JArray EnsureArrayOfInts(this JToken data)
		{
			var ints = data as JArray;
			if (ints == null)
			{
				RaiseError($"{ints} is expected to be array of integers.");
			}

			return ints;
		}

		public static Point ToPoint(this JToken data)
		{
			var ints = data.EnsureArrayOfInts();
			return new Point(ints[0].ToInt(), ints[1].ToInt());
		}

		public static Color ToColor(this JToken data)
		{
			var ints = data.EnsureArrayOfInts();
			return new Color((byte)ints[0], (byte)ints[1], (byte)ints[2], (byte)ints[3]);
		}

		public static float ToFloat(this string value)
		{
			if (!float.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out float result))
			{
				RaiseError($"Can't parse '{value}' as float value.");
			}

			return result;
		}

		public static float ToFloat(this JToken data) => ToFloat(data.ToString());

		private static JArray EnsureArrayOfFloats(this JToken data)
		{
			var floats = data as JArray;
			if (floats == null)
			{
				RaiseError($"{floats} is expected to be array of floats.");
			}

			return floats;
		}

		public static Vector2 ToVector2(this JToken data)
		{
			var floats = data.EnsureArrayOfFloats();
			return new Vector2(floats[0].ToFloat(), floats[1].ToFloat());
		}

		public static Vector3 ToVector3(this JToken data)
		{
			var floats = data.EnsureArrayOfFloats();
			return new Vector3(floats[0].ToFloat(),
				floats[1].ToFloat(),
				floats[2].ToFloat());
		}

		public static Vector4 ToVector4(this JToken data, float defW = 0.0f)
		{
			var floats = data.EnsureArrayOfFloats();
			var result = new Vector4
			{
				X = floats[0].ToFloat(),
				Y = floats[1].ToFloat(),
				Z = floats[2].ToFloat()
			};

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

		public static int EnsureInt(this JObject obj, string fieldName)
		{
			var value = obj.EnsureString(fieldName);
			return ToInt(value);
		}

		public static float EnsureFloat(this JObject obj, string fieldName)
		{
			var value = obj.EnsureString(fieldName);
			return ToFloat(value);
		}

		public static bool EnsureBool(this JObject obj, string fieldName)
		{
			var value = obj.EnsureString(fieldName);
			bool result;
			if (!bool.TryParse(value, out result))
			{
				RaiseError($"Can't parse '{value}' as bool value.");
			}

			return result;
		}

		public static Color EnsureColor(this JObject obj, string fieldName)
		{
			var colorObj = obj.EnsureJToken(fieldName);
			return colorObj.ToColor();
		}


		public static Vector3 EnsureVector3(this JObject obj, string fieldName)
		{
			var vectorObj = obj.EnsureJToken(fieldName);
			return vectorObj.ToVector3();
		}

		public static T EnsureEnum<T>(this JObject obj, string fieldName)
		{
			var value = obj.EnsureString(fieldName);

			return value.ToEnum<T>();
		}

		public static string EnsureId(this JObject obj)
		{
			return obj.EnsureString("Id");
		}

		public static JToken Optional(this JObject obj, string fieldName)
		{
			return obj[fieldName];
		}

		public static JObject OptionalJObject(this JObject obj, string fieldName)
		{
			return (JObject)Optional(obj, fieldName);
		}

		public static JArray OptionalJArray(this JObject obj, string fieldName)
		{
			return (JArray)Optional(obj, fieldName);
		}

		public static string OptionalString(this JObject obj, string fieldName, string def = null)
		{
			var token = Optional(obj, fieldName);
			if (token == null)
			{
				return def;
			}
			return token.ToString();
		}

		public static string OptionalId(this JObject obj)
		{
			return obj.OptionalString("Id");
		}

		public static int? OptionalNullableInt(this JObject obj, string fieldName, int? def = 0)
		{
			var value = OptionalString(obj, fieldName);
			if (value == null)
			{
				return def;
			}

			return ToInt(value);
		}

		public static int OptionalInt(this JObject obj, string fieldName, int def = 0)
		{
			var value = OptionalString(obj, fieldName);
			if (value == null)
			{
				return def;
			}

			return ToInt(value);
		}

		public static float OptionalFloat(this JObject obj, string fieldName, float def = 0)
		{
			var value = OptionalString(obj, fieldName);
			if (value == null)
			{
				return def;
			}

			return ToFloat(value);
		}

		public static bool OptionalBool(this JObject obj, string fieldName, bool def)
		{
			var value = OptionalString(obj, fieldName);
			if (value == null)
			{
				return def;
			}

			bool result;
			if (!bool.TryParse(value, out result))
			{
				RaiseError($"Can't parse '{value}' as bool value.");
			}

			return result;
		}

		public static Point OptionalPoint(this JObject obj, string fieldName, int defX = 0, int defY = 0)
		{
			var value = obj.Optional(fieldName);
			if (value == null)
			{
				return new Point(defX, defY);
			}

			return value.ToPoint();
		}

		public static Vector2 OptionalVector2(this JObject obj, string fieldName, float x = 0, float y = 0)
		{
			var value = obj.Optional(fieldName);
			if (value == null)
			{
				return new Vector2(x, y);
			}

			return value.ToVector2();
		}

		public static Vector3 OptionalVector3(this JObject obj, string fieldName, Vector3 def)
		{
			var value = obj.Optional(fieldName);
			if (value == null)
			{
				return def;
			}

			return value.ToVector3();
		}

		public static Vector4 OptionalVector4(this JObject obj, string fieldName, Vector4 def)
		{
			var value = obj.Optional(fieldName);
			if (value == null)
			{
				return def;
			}

			return value.ToVector4();
		}

		public static string ToJsonString(this float f)
		{
			return f.ToString(CultureInfo.InvariantCulture);
		}

		public static JArray ToJArray(this Point p)
		{
			var result = new JArray
			{
				p.X,
				p.Y
			};

			return result;
		}

		public static JArray ToJArray(this Color c)
		{
			var result = new JArray
			{
				(int)c.R,
				(int)c.G,
				(int)c.B,
				(int)c.A
			};

			return result;
		}

		public static JArray ToJArray(this Vector2 v)
		{
			var result = new JArray
			{
				v.X.ToJsonString(),
				v.Y.ToJsonString()
			};

			return result;
		}

		public static JArray ToJArray(this Vector3 v)
		{
			var result = new JArray
			{
				v.X.ToJsonString(),
				v.Y.ToJsonString(),
				v.Z.ToJsonString()
			};

			return result;
		}

		public static void RaiseError(string message)
		{
			throw new Exception(message);
		}
	}
}