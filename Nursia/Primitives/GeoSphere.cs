using Nursia.Utilities;
using Nursia.Rendering;
using Nursia.Data.Meshes;

namespace Nursia.Primitives
{
	public class GeoSphere : PrimitiveMeshNode
	{
		private float _radius = 0.5f;
		private int _tessellation = 3;

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

		protected override Mesh CreateMesh() => MeshHelper.CreateGeoSphere(Radius, Tessellation, UScale, VScale, IsLeftHanded);
	}
}