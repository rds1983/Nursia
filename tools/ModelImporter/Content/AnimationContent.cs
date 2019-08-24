using System;
using System.Collections.Generic;

namespace Nursia.ModelImporter.Content
{
	class AnimationContent: BaseContent
	{
		private readonly Dictionary<string, AnimationChannel> _channels = new Dictionary<string, AnimationChannel>();

		public Dictionary<string, AnimationChannel> Channels
		{
			get
			{
				return _channels;
			}
		}

		public TimeSpan Duration { get; set; }
	}
}
