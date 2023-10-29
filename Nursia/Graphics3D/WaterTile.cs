using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;

namespace Nursia.Graphics3D
{
	public enum WaterRenderMode
	{
		Color,
		RefractionTexture,
		ReflectionTexture,
		DepthTexture
	}

	public class WaterTile: ItemWithId
	{
		[Category("Position")]
		public float X { get; set; }

		[Category("Position")]
		public float Z { get; set; }

		[Category("Position")]
		public float Height { get; set; }

		[Category("Position")]
		public float SizeX { get; set; }

		[Category("Position")]
		public float SizeZ { get; set; }

		[Category("Behavior")]
		public Color ColorDeep { get; set; } = new Color(13, 75, 100);

		[Category("Behavior")]
		public Color ColorShallow { get; set; } = new Color(0, 141, 166);

		[Category("Behavior")]
		public Vector2 WaveDirection1 { get; set; } = new Vector2(2, 0);

		[Category("Behavior")]
		public Vector2 WaveDirection2 { get; set; } = new Vector2(0, 1);

		[Category("Behavior")]
		public float ReflectionDistortion { get; set; } = 0.3f;

		[Category("Behavior")]
		public float TimeScale { get; set; } = 0.025f;

		[Category("Behavior")]
		public float SpecularFactor { get; set; } = 0.0f;

		[Category("Behavior")]
		public float SpecularPower { get; set; } = 250.0f;

		[Category("Behavior")]
		public float EdgeFactor { get; set; } = 1.0f;

		[Category("Behavior")]
		public float MurkinessStart { get; set; } = 0.0f;

		[Category("Behavior")]
		public float MurkinessFactor { get; set; } = 5.0f;

		[Category("Behavior")]
		public bool CubeMapReflection { get; set; } = false;

		[Category("Behavior")]
		public WaterRenderMode RenderMode { get; set; }

		[Browsable(false)]
		public BoundingBox BoundingBox => new BoundingBox(new Vector3(X, Height, Z), new Vector3(X + SizeX, Height, Z + SizeZ));

		internal RenderTarget2D TargetReflection;

		public WaterTile(float x, float z, float height, float sizeX = 40.0f, float sizeZ = 40.0f)
		{
			X = x;
			Z = z;
			Height = height;
			SizeX = sizeX;
			SizeZ = sizeZ;
		}
	}
}
