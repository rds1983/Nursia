using Microsoft.Xna.Framework;
using Nursia.Graphics3D.Lights;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class RenderContext
	{
		private readonly List<DirectionalLight> _lights = new List<DirectionalLight>();

		public Camera Camera
		{
			get;
			set;
		}

		public List<DirectionalLight> Lights
		{
			get
			{
				return _lights;
			}
		}

		public Matrix Transform { get; set; }

		public Matrix[] BoneTransforms { get; set; }

		public RenderContext()
		{
			Transform = Matrix.Identity;
		}
	}
}
