using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			if (Scene == null || Scene.Terrain == null)
			{
				return;
			}

			// Build viewport
			var bounds = ActualBounds;
			var p = ToGlobal(bounds.Location);
			bounds.X = p.X;
			bounds.Y = p.Y;
			var viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

			// Determine marker position
			var nearPoint = new Vector3(Desktop.MousePosition.X, Desktop.MousePosition.Y, 0);
			var farPoint = new Vector3(Desktop.MousePosition.X, Desktop.MousePosition.Y, 1);

			nearPoint = viewport.Unproject(nearPoint, Renderer.Projection, Renderer.View, Matrix.Identity);
			farPoint = viewport.Unproject(farPoint, Renderer.Projection, Renderer.View, Matrix.Identity);

			var direction = (farPoint - nearPoint);
			direction.Normalize();

			var ray = new Ray(nearPoint, direction);
			var bb = new BoundingBox(Vector3.Zero, new Vector3(Scene.Terrain.SizeX, 0.0f, Scene.Terrain.SizeZ));
			var intersectDist = ray.Intersects(bb);

			if (intersectDist != null)
			{
				var markerPosition = nearPoint + direction * intersectDist.Value;
				markerPosition.Y = Scene.Terrain.GetHeight(markerPosition.X, markerPosition.Z);

				Scene.Marker.Position = markerPosition;
			}
			else
			{
				Scene.Marker.Position = null;
			}
		}

		private void RaiseLower()
		{
			if (Scene == null || Scene.Terrain == null || Scene.Marker.Position == null)
			{
				return;
			}

			var mouse = Mouse.GetState();

			var power = 0.0f;
			if (mouse.LeftButton == ButtonState.Pressed)
			{
				power = 0.5f;
			} else if (mouse.RightButton == ButtonState.Pressed)
			{
				power = -0.5f;
			}

			if (power == 0.0f)
			{
				return;
			}

			var markerPos = Scene.Marker.Position.Value;
			for (var x = (int)((markerPos.X - Scene.Marker.Radius) * Scene.Terrain.ResolutionX);
				x <= (int)((markerPos.X + Scene.Marker.Radius) * Scene.Terrain.ResolutionX);
				++x)
			{
				for (var z = (int)((markerPos.Z - Scene.Marker.Radius) * Scene.Terrain.ResolutionZ);
					z <= (int)((markerPos.Z + Scene.Marker.Radius) * Scene.Terrain.ResolutionZ);
					++z)
				{
					var pos = new Vector3(x / (float)Scene.Terrain.ResolutionX, 0, z / (float)Scene.Terrain.ResolutionZ);
					var dist = Vector3.Distance(markerPos, pos);

					if (dist > Scene.Marker.Radius)
					{
						continue;
					}

					var height = Scene.Terrain.GetHeight(pos.X, pos.Z);
					height += power;
					Scene.Terrain.SetHeight(pos.X, pos.Z, height);
				}
			}
		}

		public override bool OnTouchDown()
		{
			var result = base.OnTouchDown();

			RaiseLower();

			return result;
		}

		public override void OnTouchMoved()
		{
			base.OnTouchMoved();

			RaiseLower();
		}
	}
}
