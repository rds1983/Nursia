using Nursia.Rendering;
using Nursia.Utilities;
using Nursia.Data.Meshes;
using Nursia.Attributes;

namespace Nursia.Primitives
{
	[EditorInfo("Primitive")]
	public class Cone : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private float _height = 1.0f;
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

		protected override Mesh CreateMesh() => MeshHelper.CreateCone(Radius, Height, Tessellation, UScale, VScale, IsLeftHanded);
	}
}