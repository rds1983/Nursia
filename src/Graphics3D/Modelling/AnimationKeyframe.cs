using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D.Modelling
{
	public class AnimationKeyframe
	{
		public TimeSpan Time { get; set; }
		public Vector3? Translation { get; set; }
		public Vector3? Scale { get; set; }
		public Quaternion? Rotation { get; set; }
	}
}
