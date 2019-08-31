using Microsoft.Xna.Framework;
using Nursia.Graphics3D.Lights;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class RenderContext
	{
		private readonly List<DirectionalLight> _lights = new List<DirectionalLight>();

		public List<DirectionalLight> Lights
		{
			get
			{
				return _lights;
			}
		}

		public Matrix Projection { get; set; } = Matrix.Identity;
		public Matrix View { get; set; } = Matrix.Identity;

		internal Matrix ViewProjection { get; set; }
		internal Matrix World { get; set; }

		public RenderContext()
		{
			World = Matrix.Identity;
		}
	}
}
