using glTFLoader.Schema;
using static glTFLoader.Schema.Accessor;

namespace Nursia.Utilities
{
	internal static class GltfLoaderExtensions
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

		public static int GetComponentCount(this TypeEnum type) => ComponentsCount[(int)type];
		public static int GetComponentSize(this ComponentTypeEnum type) => ComponentSizes[(int)type - 5120];

		public static InterpolationMode ToInterpolationMode(this InterpolationEnum v)
		{
			switch (v)
			{
				case InterpolationEnum.LINEAR:
					return InterpolationMode.Linear;
				case InterpolationEnum.CUBICSPLINE:
					return InterpolationMode.Cubic;
			}

			return InterpolationMode.None;
		}
	}
}
