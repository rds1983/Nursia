using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Nursia.Graphics3D
{
	public class Skybox
	{
		private static readonly short[] _indices =
		{
			0, 1, 3, 1, 2, 3, 1, 5, 2,
			2, 5, 6, 4, 7, 5, 5, 7, 6,
			0, 3, 4, 4, 3, 7, 7, 3, 6,
			6, 3, 2, 4, 5, 0, 0, 5, 1
		};

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
			_mesh = Mesh.Create(GenerateCube(size), _indices);
		}

		private static VertexPositionTexture[] GenerateCube(int size)
		{
			return new VertexPositionTexture[]
			{
				new VertexPositionTexture(new Vector3(-size, size, size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(size, size, size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(size, -size, size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(-size, -size, size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(-size, size, -size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(size, size, -size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(size, -size, -size), Vector2.Zero),
				new VertexPositionTexture(new Vector3(-size, -size,-size), Vector2.Zero)
			};
		}
	}
}