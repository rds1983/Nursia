using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using Nursia.Graphics3D.Terrain;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Scene
	{
		private readonly Camera _camera = new Camera();
		private readonly List<DirectLight> _lights = new List<DirectLight>();
		private readonly List<Sprite3D> _models = new List<Sprite3D>();
		private readonly List<WaterTile> _waterTiles = new List<WaterTile>();
		private readonly List<TerrainTile> _terrainTiles = new List<TerrainTile>();

		public List<DirectLight> Lights
		{
			get
			{
				return _lights;
			}
		}

		public Skybox Skybox;

		public List<Sprite3D> Models
		{
			get
			{
				return _models;
			}
		}

		public List<WaterTile> WaterTiles
		{
			get
			{
				return _waterTiles;
			}
		}

		public List<TerrainTile> TerrainTiles
		{
			get
			{
				return _terrainTiles;
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
