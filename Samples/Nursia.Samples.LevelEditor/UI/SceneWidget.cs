using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;

namespace Nursia.UI
{
	public class SceneWidget : Widget
	{
		private readonly ForwardRenderer _renderer = new ForwardRenderer();

		public Scene Scene { get; set; }
		public ForwardRenderer Renderer { get => _renderer; }

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			if (Scene == null)
			{
				return;
			}

			var bounds = ActualBounds;
			var device = Nrs.GraphicsDevice;
			var oldViewport = device.Viewport;

			var p = ToGlobal(bounds.Location);
			bounds.X = p.X;
			bounds.Y = p.Y;

			try
			{
				device.Viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

				_renderer.Begin();
				_renderer.DrawScene(Scene);
				_renderer.End();
			}
			finally
			{
				device.Viewport = oldViewport;
			}
		}
	}
}
