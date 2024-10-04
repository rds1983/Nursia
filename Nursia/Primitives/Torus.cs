using Nursia.Rendering;
using Nursia.Utilities;
using Nursia.Data.Meshes;

namespace Nursia.Primitives
{
	public class Torus : PrimitiveMeshNode
	{
		private float _majorRadius = 0.5f;
		private float _minorRadius = 0.16666f;
		private int _tessellation = 32;

		public float MajorRadius
		{
			get => _majorRadius;

			set
			{
				if (value.EpsilonEquals(_majorRadius))
				{
					return;
				}

				_majorRadius = value;
				InvalidateMesh();
			}
		}

		public float MinorRadius
		{
			get => _minorRadius;

			set
			{
				if (value.EpsilonEquals(_minorRadius))
				{
					return;
				}

				_minorRadius = value;
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

		protected override Mesh CreateMesh() => MeshHelper.CreateTorus(MajorRadius, MinorRadius, Tessellation, UScale, VScale, IsLeftHanded);
	}
}