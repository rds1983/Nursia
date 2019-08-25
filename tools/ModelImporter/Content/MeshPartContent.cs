using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class MeshPartContent
	{
		private readonly List<short> _indices = new List<short>();

		public List<short> Indices
		{
			get
			{
				return _indices;
			}
		}

		public MaterialContent Material { get; set; }

		public object[,] Vertices { get; set; }

		public VertexDeclaration VertexDeclaration { get; set; }

		public Dictionary<string, List<BoneWeight>> BoneWeights { get; set; }

		public int ElementsPerRowWithoutBones { get; set; }
		public int ElementsPerRow { get; set; }
	}
}
