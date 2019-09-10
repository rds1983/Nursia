using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Scene
	{
		private readonly List<DirectLight> _lights = new List<DirectLight>();
		private readonly List<Sprite3D> _models = new List<Sprite3D>();
		private readonly Camera _camera = new Camera();

		public List<DirectLight> Lights
		{
			get
			{
				return _lights;
			}
		}

		public List<Sprite3D> Models
		{
			get
			{
				return _models;
			}
		}

		public Camera Camera
		{
			get
			{
				return _camera;
			}
		}
	}
}
