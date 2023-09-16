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
		public bool Waves { get; set; } = true;

		[Category("Behavior")]
		public bool SoftEdges { get; set; } = true;

		[Category("Behavior")]
		public Color Color { get; set; } = new Vector4(0.5f, 0.79f, 0.75f, 1.0f).ToColor();

		[Category("Behavior")]
		public float WaveTextureScale { get; set; } = 2.5f;

		[Category("Behavior")]
		public float SpecularFactor { get; set; } = 0.0f;

		[Category("Behavior")]
		public float SpecularPower { get; set; } = 250.0f;

		[Category("Behavior")]
		public float FresnelFactor { get; set; } = 1.0f;

		[Category("Behavior")]
		public float EdgeFactor { get; set; } = 1.0f;

		[Category("Behavior")]
		public Vector2 WaveVelocity0 { get; set; } = new Vector2(0.01f, 0.03f);

		[Category("Behavior")]
		public Vector2 WaveVelocity1 { get; set; } = new Vector2(-0.01f, 0.03f);

		internal Vector2 WaveMapOffset0;
		internal Vector2 WaveMapOffset1;

		[Category("Behavior")]
		public WaterRenderMode RenderMode { get; set; }

		public BoundingBox BoundingBox => new BoundingBox(new Vector3(X, Height, Z), new Vector3(X + SizeX, Height, Z + SizeZ));

		internal RenderTarget2D TargetRefraction;
		internal RenderTarget2D TargetReflection;
		internal RenderTarget2D TargetDepth;



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
