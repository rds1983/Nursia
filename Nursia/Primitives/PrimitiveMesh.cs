using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Primitives
{
	public abstract class PrimitiveMesh
	{
		protected class Builder
		{
			public List<VertexPositionNormalTexture> Vertices { get; set; } = new List<VertexPositionNormalTexture>();
			public List<short> Indices { get; } = new List<short>();

			public Mesh Create(bool toLeftHanded)
			{
				if (toLeftHanded)
				{
					for (var i = 0; i < Indices.Count; i += 3)
					{
						var temp = Indices[i];
						Indices[i] = Indices[i + 2];
						Indices[i + 2] = temp;
					}

					for (var i = 0; i < Vertices.Count; ++i)
					{
						var v = Vertices[i];
						v.TextureCoordinate.X = (1.0f - v.TextureCoordinate.X);

						Vertices[i] = v;
					}
				}

				return new Mesh(Vertices.ToArray(), Indices.ToArray());
			}
		}

		private bool _isLeftHanded;
		private Mesh _mesh;

		[Browsable(false)]
		[JsonIgnore]
		public Mesh Mesh
		{
			get
			{
				if (_mesh == null)
				{
					_mesh = CreateMesh();
				}

				return _mesh;
			}
		}

		public bool IsLeftHanded
		{
			get => _isLeftHanded;

			set
			{
				if (value == _isLeftHanded)
				{
					return;
				}

				_isLeftHanded = value;
				InvalidateMesh();
			}
		}

		protected abstract Mesh CreateMesh();

		public void InvalidateMesh()
		{
			_mesh = null;
		}
	}
}