using Nursia.Utilities;
using Nursia.Rendering;
using Nursia.Data.Meshes;
using Nursia.Attributes;

namespace Nursia.Primitives
{
	[EditorInfo("Primitive")]
	public class Teapot : PrimitiveMeshNode
	{
		private float _size = 1.0f;
		private int _tessellation = 8;

		public float Size
		{
			get => _size;

			set
			{
				if (value.EpsilonEquals(_size))
				{
					return;
				}

				_size = value;
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

		protected override Mesh CreateMesh() => MeshHelper.CreateTeapot(Size, Tessellation, UScale, VScale, IsLeftHanded);
	}
}