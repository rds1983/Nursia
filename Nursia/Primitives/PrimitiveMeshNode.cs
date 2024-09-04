using Newtonsoft.Json;
using Nursia.Rendering;

namespace Nursia.Primitives
{
	public enum PrimitiveMeshType
	{
		Capsule,
		Cube,
		Cylinder,
		GeoSphere,
		Plane,
		Sphere,
		Teapot,
		Torus
	}

	public class PrimitiveMeshNode: MeshNode
	{
		public PrimitiveMesh PrimitiveMesh { get; set; }

		[JsonIgnore]
		public PrimitiveMeshType? PrimitiveMeshType
		{
			get
			{
				if (PrimitiveMesh is Capsule)
				{
					return Primitives.PrimitiveMeshType.Capsule;
				}
				else if (PrimitiveMesh is Cube)
				{
					return Primitives.PrimitiveMeshType.Cube;
				}
				else if (PrimitiveMesh is Cylinder)
				{
					return Primitives.PrimitiveMeshType.Cylinder;
				}
				else if (PrimitiveMesh is GeoSphere)
				{
					return Primitives.PrimitiveMeshType.GeoSphere;
				}
				else if (PrimitiveMesh is Plane)
				{
					return Primitives.PrimitiveMeshType.Plane;
				}
				else if (PrimitiveMesh is Sphere)
				{
					return Primitives.PrimitiveMeshType.Sphere;
				}
				else if (PrimitiveMesh is Teapot)
				{
					return Primitives.PrimitiveMeshType.Teapot;
				}
				else if (PrimitiveMesh is Torus)
				{
					return Primitives.PrimitiveMeshType.Torus;
				}

				return null;
			}

			set
			{
				if (value == PrimitiveMeshType)
				{
					return;
				}

				switch (value)
				{
					case Primitives.PrimitiveMeshType.Capsule:
						PrimitiveMesh = new Capsule();
						break;
					case Primitives.PrimitiveMeshType.Cube:
						PrimitiveMesh = new Cube();
						break;
					case Primitives.PrimitiveMeshType.Cylinder:
						PrimitiveMesh = new Cylinder();
						break;
					case Primitives.PrimitiveMeshType.GeoSphere:
						PrimitiveMesh = new GeoSphere();
						break;
					case Primitives.PrimitiveMeshType.Plane:
						PrimitiveMesh = new Plane();
						break;
					case Primitives.PrimitiveMeshType.Sphere:
						PrimitiveMesh = new Sphere();
						break;
					case Primitives.PrimitiveMeshType.Teapot:
						PrimitiveMesh = new Teapot();
						break;
					case Primitives.PrimitiveMeshType.Torus:
						PrimitiveMesh = new Torus();
						break;
					default:
						PrimitiveMesh = null;
						break;
				}
			}
		}

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			Mesh = PrimitiveMesh?.Mesh;
		}
	}
}
