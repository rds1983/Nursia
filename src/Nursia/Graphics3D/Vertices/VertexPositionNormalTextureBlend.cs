using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace Nursia.Graphics3D.Utils.Vertices
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionNormalTextureBlend : IVertexType
	{
		public Vector3 Position;
		public Vector3 Normal;
		public Vector2 TextureCoordinate;
		public Vector4 BlendWeight;
		public Vector4 BlendIndex;

		public static readonly VertexDeclaration VertexDeclaration;

		public VertexPositionNormalTextureBlend(Vector3 position,
			Vector3 normal,
			Vector2 textureCoordinate,
			Vector4 blendWeight,
			Vector4 blendIndex)
		{
			Position = position;
			Normal = normal;
			TextureCoordinate = textureCoordinate;
			BlendWeight = blendWeight;
			BlendIndex = blendIndex;
		}

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get
			{
				return VertexDeclaration;
			}
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Position.GetHashCode();
				hashCode = (hashCode * 397) ^ Normal.GetHashCode();
				hashCode = (hashCode * 397) ^ TextureCoordinate.GetHashCode();
				hashCode = (hashCode * 397) ^ BlendWeight.GetHashCode();
				hashCode = (hashCode * 397) ^ BlendIndex.GetHashCode();
				return hashCode;
			}
		}

		public override string ToString()
		{
			return "{{Position:" + Position +
				" Normal:" + Normal +
				" TextureCoordinate:" + TextureCoordinate +
				" BlendWeight:" + BlendWeight +
				" BlendIndex:" + BlendIndex +
				"}}";
		}

		public static bool operator ==(VertexPositionNormalTextureBlend left, VertexPositionNormalTextureBlend right)
		{
			return left.Position == right.Position &&
				left.Normal == right.Normal &&
				left.TextureCoordinate == right.TextureCoordinate &&
				left.BlendWeight == right.BlendWeight &&
				left.BlendIndex == right.BlendIndex;
		}

		public static bool operator !=(VertexPositionNormalTextureBlend left, VertexPositionNormalTextureBlend right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return (this == ((VertexPositionNormalTextureBlend)obj));
		}

		static VertexPositionNormalTextureBlend()
		{
			VertexElement[] elements = new VertexElement[] {
				new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
				new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
				new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
				new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
				new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendIndices, 0)
			};

			VertexDeclaration declaration = new VertexDeclaration(elements);
			VertexDeclaration = declaration;
		}
	}
}