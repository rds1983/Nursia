using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Nursia.Graphics3D
{
	public class Skybox
	{
		private readonly Mesh _mesh;

		[Browsable(false)]
		[XmlIgnore]
		public Mesh Mesh
		{
			get
			{
				return _mesh;
			}
		}

		[Browsable(false)]
		[XmlIgnore]
		public TextureCube Texture;

		public Skybox(int size = 500)
		{
			_mesh = Mesh.Create(GenerateCube(size), PrimitivesFactory.BoxIndices);
		}

		private static VertexPositionTexture[] GenerateCube(int size)
		{
			var vectors = PrimitivesFactory.CreateBox(new Vector3(-size, -size, -size), new Vector3(size, size, size));

			var verticesList = new List<VertexPositionTexture>();
			for (var i = 0; i < vectors.Length; ++i)
			{
				verticesList.Add(new VertexPositionTexture(vectors[i], Vector2.Zero));
			}

			return verticesList.ToArray();
		}
	}
}