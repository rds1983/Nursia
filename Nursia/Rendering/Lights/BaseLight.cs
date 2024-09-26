using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.Rendering.Lights
{
	/// <summary>
	/// Base Light
	/// </summary>
	public abstract class BaseLight : SceneNode
	{
		[Browsable(false)]
		[JsonIgnore]
		public abstract bool RenderCastsShadow { get; }
		
		public Color Color { get; set; } = Color.White;

		public abstract Camera GetLightCamera(Vector3 viewerPos);
	}
}
