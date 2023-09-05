using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Utils;
using static Nursia.Graphics3D.Utils.CameraInputController;

namespace Nursia.Samples.LevelEditor.UI
{
	public class SceneWidget : Widget
	{
		private Scene _scene;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private CameraInputController _controller;

		public Scene Scene
		{
			get => _scene;
			set
			{
				if (_scene == value) return;

				_scene = value;
				_controller = _scene == null ? null : new CameraInputController(_scene.Camera);
			}
		}
		public ForwardRenderer Renderer { get => _renderer; }
		public Instrument Instrument { get; } = new Instrument();

		private Vector3? CalculateMarkerPosition()
		{
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

			// Firstly determine whether we intersect zero height terrain rectangle
			var bb = Utils.CreateBoundingBox(0, Scene.Terrain.SizeX, 0, 0, 0, Scene.Terrain.SizeZ);
			var intersectDist = ray.Intersects(bb);
			if (intersectDist == null)
			{
				return null;
			}

			var markerPosition = nearPoint + direction * intersectDist.Value;

			// Now determine where we intersect terrain rectangle with real height
			var height = Scene.Terrain.GetHeight(markerPosition.X, markerPosition.Z);
			bb = Utils.CreateBoundingBox(0, Scene.Terrain.SizeX, height, height, 0, Scene.Terrain.SizeZ);
			intersectDist = ray.Intersects(bb);
			if (intersectDist == null)
			{
				return null;
			}
			
			markerPosition = nearPoint + direction * intersectDist.Value;

			return markerPosition;
		}

		private void UpdateMarker()
		{
			if (Scene == null || Scene.Terrain == null)
			{
				return;
			}

			switch (Instrument.Type)
			{
				case InstrumentType.None:
					Scene.Marker.Position = null;
					break;
				case InstrumentType.RaiseTerrain:
					Scene.Marker.Position = CalculateMarkerPosition();
					break;
				case InstrumentType.LowerTerrain:
					Scene.Marker.Position = CalculateMarkerPosition();
					break;
			}

			Scene.Marker.Radius = Instrument.Power;
		}

		private void UpdateKeyboard()
		{
			var keyboardState = Keyboard.GetState();

			// Manage camera input controller
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Left, keyboardState.IsKeyDown(Keys.A));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Right, keyboardState.IsKeyDown(Keys.D));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Forward, keyboardState.IsKeyDown(Keys.W));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Backward, keyboardState.IsKeyDown(Keys.S));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Up, keyboardState.IsKeyDown(Keys.Up));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Down, keyboardState.IsKeyDown(Keys.Down));
			_controller.Update();
		}

		public override void InternalRender(RenderContext context)
		{
			base.InternalRender(context);

			if (Scene == null)
			{
				return;
			}

			UpdateKeyboard();

			var bounds = ActualBounds;
			var device = Nrs.GraphicsDevice;
			var oldViewport = device.Viewport;

			var p = ToGlobal(bounds.Location);
			bounds.X = p.X;
			bounds.Y = p.Y;

			try
			{
				device.Viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

				UpdateMarker();
				_renderer.Begin();
				_renderer.DrawScene(Scene);
				_renderer.End();
			}
			finally
			{
				device.Viewport = oldViewport;
			}
		}

		private void RaiseLower()
		{
			if (Scene == null || Scene.Terrain == null || Scene.Marker.Position == null)
			{
				return;
			}

			var mouse = Mouse.GetState();
			if (mouse.LeftButton != ButtonState.Pressed)
			{
				return;
			}

			var power = 0.0f;
			if (Instrument.Type == InstrumentType.RaiseTerrain)
			{
				power = 0.5f;
			}
			else if (Instrument.Type == InstrumentType.LowerTerrain)
			{
				power = -0.5f;
			}

			if (power == 0.0f)
			{
				return;
			}

			var radius = Scene.Marker.Radius;
			var markerPos = Scene.Marker.Position.Value;
			for (var x = (int)((markerPos.X - radius) * Scene.Terrain.ResolutionX);
				x <= (int)((markerPos.X + radius) * Scene.Terrain.ResolutionX);
				++x)
			{
				for (var z = (int)((markerPos.Z - radius) * Scene.Terrain.ResolutionZ);
					z <= (int)((markerPos.Z + radius) * Scene.Terrain.ResolutionZ);
					++z)
				{
					var pos = new Vector2(x / (float)Scene.Terrain.ResolutionX, z / (float)Scene.Terrain.ResolutionZ);
					var dist = Vector2.Distance(new Vector2(markerPos.X, markerPos.Z), pos);

					if (dist > radius)
					{
						continue;
					}

					var height = Scene.Terrain.GetHeight(pos.X, pos.Y);
					height += power;
					Scene.Terrain.SetHeight(pos.X, pos.Y, height);
				}
			}
		}

		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			var mouseState = Mouse.GetState();
			_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));
		}

		public override bool OnTouchDown()
		{
			var result = base.OnTouchDown();

			RaiseLower();

			return result;
		}

		public override void OnTouchUp()
		{
			base.OnTouchUp();
			_controller.SetTouchState(TouchType.Move, false);
			_controller.SetTouchState(TouchType.Rotate, false);
		}

		public override void OnTouchMoved()
		{
			base.OnTouchMoved();

			var mouseState = Mouse.GetState();
			if (Instrument.Type == InstrumentType.None)
			{
				_controller.SetTouchState(TouchType.Move, mouseState.LeftButton == ButtonState.Pressed);
			}

			_controller.SetTouchState(TouchType.Rotate, mouseState.RightButton == ButtonState.Pressed);
			_controller.Update();

			RaiseLower();
		}
	}
}
