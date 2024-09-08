using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Standard;
using System.ComponentModel;

namespace Nursia.Primitives
{
	public enum PrimitiveMeshType
	{
		Capsule,
		Cone,
		Cube,
		Cylinder,
		Disc,
		GeoSphere,
		Plane,
		Sphere,
		Teapot,
		Torus
	}

	public class PrimitiveMeshNode : MeshNodeBase
	{
		public PrimitiveMesh PrimitiveMesh { get; set; }

		[JsonIgnore]
		public PrimitiveMeshType? MeshType
		{
			get
			{
				if (PrimitiveMesh is Capsule)
				{
					return PrimitiveMeshType.Capsule;
				}
				else if (PrimitiveMesh is Cone)
				{
					return PrimitiveMeshType.Cone;
				}
				else if (PrimitiveMesh is Cube)
				{
					return PrimitiveMeshType.Cube;
				}
				else if (PrimitiveMesh is Cylinder)
				{
					return PrimitiveMeshType.Cylinder;
				}
				else if (PrimitiveMesh is Disc)
				{
					return PrimitiveMeshType.Disc;
				}
				else if (PrimitiveMesh is GeoSphere)
				{
					return PrimitiveMeshType.GeoSphere;
				}
				else if (PrimitiveMesh is Plane)
				{
					return PrimitiveMeshType.Plane;
				}
				else if (PrimitiveMesh is Sphere)
				{
					return PrimitiveMeshType.Sphere;
				}
				else if (PrimitiveMesh is Teapot)
				{
					return PrimitiveMeshType.Teapot;
				}
				else if (PrimitiveMesh is Torus)
				{
					return PrimitiveMeshType.Torus;
				}

				return null;
			}

			set
			{
				if (value == MeshType)
				{
					return;
				}

				switch (value)
				{
					case PrimitiveMeshType.Capsule:
						PrimitiveMesh = new Capsule();
						break;
					case PrimitiveMeshType.Cone:
						PrimitiveMesh = new Cone();
						break;
					case PrimitiveMeshType.Cube:
						PrimitiveMesh = new Cube();
						break;
					case PrimitiveMeshType.Cylinder:
						PrimitiveMesh = new Cylinder();
						break;
					case PrimitiveMeshType.Disc:
						PrimitiveMesh = new Disc();
						break;
					case PrimitiveMeshType.GeoSphere:
						PrimitiveMesh = new GeoSphere();
						break;
					case PrimitiveMeshType.Plane:
						PrimitiveMesh = new Plane();
						break;
					case PrimitiveMeshType.Sphere:
						PrimitiveMesh = new Sphere();
						break;
					case PrimitiveMeshType.Teapot:
						PrimitiveMesh = new Teapot();
						break;
					case PrimitiveMeshType.Torus:
						PrimitiveMesh = new Torus();
						break;
					default:
						PrimitiveMesh = null;
						break;
				}
			}
		}

		[DefaultValue(true)]
		public bool CastsShadow
		{
			get => PrimitiveMesh.Mesh.CastsShadow;

			set => PrimitiveMesh.Mesh.CastsShadow = value;
		}

		protected override Mesh RenderMesh => PrimitiveMesh?.Mesh;
	}
}
