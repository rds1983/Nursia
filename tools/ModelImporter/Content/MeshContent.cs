using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class MeshContent: BaseContent
	{
		private readonly List<short> _indices = new List<short>();
		private readonly List<BoneContent> _bones = new List<BoneContent>();

		public List<BoneContent> Bones
		{
			get
			{
				return _bones;
			}
		}

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

		public int ElementsPerRowWithoutBones { get; set; }
		public int ElementsPerRow { get; set; }

		public int BonesCount { get; set; }
	}
}
