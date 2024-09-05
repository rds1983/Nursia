using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Nursia.Rendering;
using Nursia.Utilities;
using System.Collections.Generic;
using System.ComponentModel;

namespace Nursia.Primitives
{
	public abstract class PrimitiveMesh
	{
		protected class Builder
		{
			public List<VertexPositionNormalTexture> Vertices { get; set; } = new List<VertexPositionNormalTexture>();
			public List<int> Indices { get; } = new List<int>();

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

				var indicesShort = new short[Indices.Count];
				for (var i = 0; i < indicesShort.Length; ++i)
				{
					indicesShort[i] = (short)Indices[i];
				}

				return new Mesh(Vertices.ToArray(), indicesShort);
			}
		}

		private bool _isLeftHanded;
		private Mesh _mesh;
		private float _uScale = 1.0f;
		private float _vScale = 1.0f;

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

		public float UScale
		{
			get => _uScale;

			set
			{
				if (value.EpsilonEquals(_uScale))
				{
					return;
				}

				_uScale = value;
				InvalidateMesh();
			}
		}

		public float VScale
		{
			get => _vScale;

			set
			{
				if (value.EpsilonEquals(_vScale))
				{
					return;
				}

				_vScale = value;
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