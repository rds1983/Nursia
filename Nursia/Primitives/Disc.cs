using Microsoft.Xna.Framework;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using Nursia.Data.Meshes;
using Nursia.Attributes;

namespace Nursia.Primitives
{
	/// <summary>
	/// A disc - a circular base, or a circular sector.
	/// </summary>
	[EditorInfo("Primitive")]
	public class Disc : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private float _sectorAngle = 360;
		private int _tessellation = 16;

		public float Radius
		{
			get => _radius;

			set
			{
				if (value.EpsilonEquals(_radius))
				{
					return;
				}

				_radius = value;
				InvalidateMesh();
			}
		}

		public float SectorAngle
		{
			get => _sectorAngle;

			set
			{
				if (value.EpsilonEquals(_sectorAngle))
				{
					return;
				}

				_sectorAngle = value;
				InvalidateMesh();
			}
		}

		public int Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		protected override Mesh CreateMesh() => MeshHelper.CreateDisc(Radius, MathHelper.ToRadians(SectorAngle), Tessellation, UScale, VScale, IsLeftHanded);
	}
}
