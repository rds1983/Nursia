using Nursia.Graphics3D.Landscape;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Scene
	{
		private readonly Camera _camera = new Camera();
		private readonly List<DirectLight> _lights = new List<DirectLight>();
		private readonly List<NursiaModel> _models = new List<NursiaModel>();
		private readonly List<WaterTile> _waterTiles = new List<WaterTile>();

		public List<DirectLight> Lights => _lights;

		public Skybox Skybox;

		public List<NursiaModel> Models => _models;

		public List<WaterTile> WaterTiles => _waterTiles;

		public Terrain Terrain { get; set; }

		public Camera Camera => _camera;
	}
}
