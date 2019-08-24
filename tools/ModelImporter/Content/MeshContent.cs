using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class MeshContent: NodeContent
	{
		private readonly List<MeshPartContent> _parts = new List<MeshPartContent>();

		public List<MeshPartContent> Parts
		{
			get
			{
				return _parts;
			}
		}
	}
}
