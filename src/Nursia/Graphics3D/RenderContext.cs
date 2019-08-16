using Nursia.Graphics3D.Lights;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class RenderContext
	{
		private readonly List<DirectionalLight> _lights = new List<DirectionalLight>();
		private readonly List<Mesh> _meshes = new List<Mesh>();

		public Camera Camera
		{
			get;
			internal set;
		}

		public List<DirectionalLight> Lights
		{
			get
			{
				return _lights;
			}
		}

		public List<Mesh> Meshes
		{
			get
			{
				return _meshes;
			}
		}
	}
}
