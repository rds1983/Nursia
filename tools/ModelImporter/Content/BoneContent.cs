using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class BoneContent: BaseContent
	{
		private readonly Dictionary<string, AnimationContent> _animations = new Dictionary<string, AnimationContent>();

		public int BoneId { get; set; }

		public Matrix Transform
		{
			get; set;
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
