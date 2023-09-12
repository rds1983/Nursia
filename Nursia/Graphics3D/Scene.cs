using Nursia.Graphics3D.Landscape;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Scene
	{
		public List<DirectLight> DirectLights { get; } = new List<DirectLight>();
		public List<BaseLight> PointLights { get; } = new List<BaseLight>();
		public bool HasLights => DirectLights.Count > 0 || PointLights.Count > 0;

		public Skybox Skybox;

		public List<ModelInstance> Models { get; } = new List<ModelInstance>();
		public List<WaterTile> WaterTiles { get; } = new List<WaterTile>();
		public float DefaultWaterLevel { get; set; } = -2;

		public Terrain Terrain { get; set; }

		public Camera Camera { get; } = new Camera();
		public EditorMarker Marker { get; } = new EditorMarker();
		public bool HasMarker => Marker.Position != null;
	}
}
