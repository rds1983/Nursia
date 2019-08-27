using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class BoneNodeContent: BaseContent
	{
		private readonly List<BoneNodeContent> _children = new List<BoneNodeContent>();

		public Matrix Transform
		{
			get; set;
		}

		public List<BoneNodeContent> Children
		{
			get
			{
				return _children;
			}
		}
	}
}
