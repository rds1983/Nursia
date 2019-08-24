using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class NodeContent: BaseContent
	{
		private readonly List<NodeContent> _children = new List<NodeContent>();
		private readonly Dictionary<string, AnimationContent> _animations = new Dictionary<string, AnimationContent>();

		public List<NodeContent> Children
		{
			get
			{
				return _children;
			}
		}

		public NodeContent Parent { get; set; }

		public Matrix Transform
		{
			get; set;
		}

		public Matrix AbsoluteTransform
		{
			get
			{
				if (Parent != null)
				{
					return Transform * Parent.AbsoluteTransform;
				}

				return Transform;
			}
		}

		public Dictionary<string, AnimationContent> Animations
		{
			get
			{
				return _animations;
			}
		}
	}
}
