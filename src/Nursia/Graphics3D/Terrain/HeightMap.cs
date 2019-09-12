using Nursia.Utilities;
using System;

namespace Nursia.Graphics3D.Terrain
{
	public class HeightMap
	{
		private const float MaxPixelColor = 256 * 256 * 256;
		private const float MaxHeight = 100;

		private float[,] _heights;

		public int Size
		{
			get
			{
				return _heights.GetLength(0);
			}
		}

		public float this[int x, int z]
		{
			get
			{
				if (x < 0 || z < 0 || x >= Size || z >= Size)
				{
					return 0;
				}

				return _heights[x, z];
			}
		}

		public HeightMap(Image2D image)
		{
			SetFromImage(image);
		}

		public void SetFromImage(Image2D image)
		{
			if (image == null)
			{
				throw new ArgumentNullException(nameof(image));
			}

			_heights = new float[image.Width, image.Height];

			for(var x = 0; x < image.Width; ++x)
			{
				for(var z = 0; z < image.Height; ++z)
				{
					var c = image[x, z];

					float height = (c.R << 16) + (c.G << 8) + c.B;

					height /= MaxPixelColor;
					height *= MaxHeight;
					height -= MaxHeight / 2;

					_heights[x, z] = height;
				}
			}
		}


	}
}
