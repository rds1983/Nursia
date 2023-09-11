using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Nursia.Graphics3D
{
	public class Skybox
	{
		private readonly MeshData _meshData;

		[Browsable(false)]
		[XmlIgnore]
		public MeshData MeshData => _meshData;

		[Browsable(false)]
		[XmlIgnore]
		public TextureCube Texture;

		public Matrix Transform;

		public Skybox(int size = 500)
		{
			_meshData = PrimitiveMeshes.CubePositionFromMinusOneToOne;
			Transform = Matrix.CreateScale(size);
		}
	}
}