using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class MeshContent: BaseContent
	{
		private readonly List<MeshPartContent> _parts = new List<MeshPartContent>();

		public List<MeshPartContent> Parts
		{
			get
			{
				return _parts;
			}
		}

		public Matrix Transform
		{
			get; set;
		}
	}
}
