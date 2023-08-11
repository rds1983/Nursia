using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Modelling
{
	internal static class GltfLoading
	{
		public static Vector3 ToVector3(this float[] array) => new Vector3(array[0], array[1], array[2]);
		public static Vector4 ToVector4(this float[] array) => new Vector4(array[0], array[1], array[2], array[3]);
		public static Quaternion ToQuaternion(this float[] array) => new Quaternion(array[0], array[1], array[2], array[3]);
	}
}
