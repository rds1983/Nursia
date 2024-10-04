using Nursia.Utilities;
using Nursia.Rendering;
using Nursia.Data.Meshes;
using Nursia.Attributes;

namespace Nursia.Primitives
{
	[EditorInfo("Primitive")]
	public class Cylinder : PrimitiveMeshNode
	{
		private float _height = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 32;

		public float Height
		{
			get => _height;

			set
			{
				if (value.EpsilonEquals(_height))
				{
					return;
				}

				_height = value;
				InvalidateMesh();
			}
		}

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

		protected override Mesh CreateMesh() => MeshHelper.CreateCylinder(Height, Radius, Tessellation, UScale, VScale, IsLeftHanded);
	}
}
