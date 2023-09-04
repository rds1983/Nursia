using Nursia.Graphics3D.Landscape;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public class Scene
	{
		public DirectLight DirectLight { get; set; }
		public List<BaseLight> PointLights { get; } = new List<BaseLight>();

		public Skybox Skybox;

		public List<NursiaModel> Models { get; } = new List<NursiaModel>();

		public List<WaterTile> WaterTiles { get; } = new List<WaterTile>();

		public Terrain Terrain { get; set; }

		public Camera Camera { get; } = new Camera();
		public EditorMarker Marker { get; } = new EditorMarker();
		public bool HasMarker => Marker.Position != null;
	}
}
