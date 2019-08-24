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

		public float[,] Vertices { get; set; }

		public Type VertexType { get; set; }

		public List<List<BoneWeight>> BoneWeights { get; set; }
	}
}
