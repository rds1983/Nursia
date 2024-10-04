using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nursia.Vertices
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct VertexPositionNormal : IVertexType
	{
		#region Private Properties

		VertexDeclaration IVertexType.VertexDeclaration
		{
			get
			{
				return VertexDeclaration;
			}
		}

		#endregion

		#region Public Variables

		public Vector3 Position;
		public Vector3 Normal;

		#endregion

		#region Public Static Variables

		public static readonly VertexDeclaration VertexDeclaration;

		#endregion

		#region Private Static Constructor

		static VertexPositionNormal()
		{
			VertexDeclaration = new VertexDeclaration(
				new VertexElement[]
				{
					new VertexElement(
						0,
						VertexElementFormat.Vector3,
						VertexElementUsage.Position,
						0
					),
					new VertexElement(
						12,
						VertexElementFormat.Vector3,
						VertexElementUsage.Normal,
						0
					)
				}
			);
		}

		#endregion

		#region Public Constructor

		public VertexPositionNormal(Vector3 position, Vector3 normal)
		{
			Position = position;
			Normal = normal;
		}

		#endregion

		#region Public Static Operators and Override Methods

		public override int GetHashCode()
		{
			// TODO: Fix GetHashCode
			return 0;
		}

		public override string ToString()
		{
			return
				"{{Position:" + Position.ToString() +
				" Normal:" + Normal.ToString() +
				"}}"
			;
		}

		public static bool operator ==(VertexPositionNormal left, VertexPositionNormal right)
		{
			return left.Position == right.Position &&
					left.Normal == right.Normal;
		}

		public static bool operator !=(VertexPositionNormal left, VertexPositionNormal right)
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
			return this == (VertexPositionNormal)obj;
		}

		#endregion
	}
}
