using Nursia.Rendering;
using Nursia.Utilities;
using Nursia.Data.Meshes;
using Nursia.Attributes;

namespace Nursia.Primitives
{
	[EditorInfo("Primitive")]
	public class Capsule : PrimitiveMeshNode
	{
		private float _length = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 8;

		public float Length
		{
			get => _length;

			set
			{
				if (value.EpsilonEquals(_length))
				{
					return;
				}

				_length = value;
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

		protected override Mesh CreateMesh() => MeshHelper.CreateCapsule(Length, Radius, Tessellation, UScale, VScale, IsLeftHanded);
	}
}