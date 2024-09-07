using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
		public RenderTarget2D ShadowMap { get; protected set; }

		public Color Color { get; set; } = Color.White;
		internal bool ShadowMapDirty { get; set; } = true;

		public abstract Matrix CreateLightViewProjectionMatrix(RenderContext context);
	}
}
