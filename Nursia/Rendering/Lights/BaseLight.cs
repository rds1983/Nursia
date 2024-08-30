using Microsoft.Xna.Framework;

namespace Nursia.Rendering.Lights
{
	/// <summary>
	/// Base Light
	/// </summary>
	public class BaseLight : SceneNode
	{
		public Color Color { get; set; } = Color.White;
	}
}
