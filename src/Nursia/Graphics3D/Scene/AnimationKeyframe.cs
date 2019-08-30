using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D.Scene
{
	public class AnimationKeyframe
	{
		public TimeSpan Time { get; set; }
		public Matrix Transform { get; set; }
	}
}
