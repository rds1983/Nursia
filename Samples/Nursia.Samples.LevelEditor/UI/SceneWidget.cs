using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Landscape;
using Nursia.Graphics3D.Modelling;
using Nursia.Graphics3D.Utils;
using static Nursia.Graphics3D.Utils.CameraInputController;

namespace Nursia.Samples.LevelEditor.UI
{
	public class SceneWidget : Widget
	{
		private Scene _scene;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private CameraInputController _controller;
		private ModelInstance _waterMarker;
		private Vector3? _touchDownStart;

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

		private static bool IsMouseLeftButtonDown
		{
			get
			{
				var mouseState = Mouse.GetState();
				return (mouseState.LeftButton == ButtonState.Pressed);
			}
		}

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
			if (Scene == null || Scene.Terrain == null || Instrument.Type == InstrumentType.None)
			{
				return;
			}

			Scene.Marker.Position = CalculateMarkerPosition();
			Scene.Marker.Radius = Instrument.Radius;
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

		protected override void OnPlacedChanged()
		{
			base.OnPlacedChanged();
			
			if (Desktop == null)
			{
				return;
			}

			Desktop.TouchUp += Desktop_TouchUp;
		}

		private void Desktop_TouchUp(object sender, EventArgs e)
		{
			_controller.SetTouchState(TouchType.Move, false);
			_controller.SetTouchState(TouchType.Rotate, false);

			if (Instrument.Type == InstrumentType.Water && _touchDownStart != null && Scene.Marker.Position != null)
			{
				GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ);

				if (sizeX > 0 && sizeZ > 0)
				{
					var waterTile = new WaterTile(startPos.X, startPos.Z, Scene.DefaultWaterLevel, sizeX, sizeZ);
					Scene.WaterTiles.Add(waterTile);
				}

				_touchDownStart = null;
				Scene.Models.Remove(_waterMarker);
			}
		}

		private void UpdateTerrainHeight(Vector2 pos, float power)
		{
			var height = Scene.Terrain.GetHeight(pos.X, pos.Y);
			height += power;
			Scene.Terrain.SetHeight(pos.X, pos.Y, height);
		}

		private void UpdateTerrainSplatMap(Point splatPos, SplatManChannel channel, float power)
		{
			var splatValue = Scene.Terrain.GetSplatValue(splatPos, channel);
			splatValue += power * 0.5f;
			Scene.Terrain.SetSplatValue(splatPos, channel, splatValue);
		}

		private void ApplyLowerRaise()
		{
			var power = Instrument.Power;
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

					switch (Instrument.Type)
					{
						case InstrumentType.None:
							break;
						case InstrumentType.RaiseTerrain:
							UpdateTerrainHeight(pos, power);
							break;
						case InstrumentType.LowerTerrain:
							UpdateTerrainHeight(pos, -power);
							break;
					}
				}
			}
		}

		private void ApplyPaint()
		{
			var power = Instrument.Power;
			var radius = Scene.Marker.Radius;
			var markerPos = Scene.Marker.Position.Value;

			var topLeft = Scene.Terrain.ToSplatPosition(markerPos.X - radius, markerPos.Z - radius);
			var bottomRight = Scene.Terrain.ToSplatPosition(markerPos.X + radius, markerPos.Z + radius);

			for (var x = topLeft.X; x <= bottomRight.X; ++x)
			{
				for (var y = topLeft.Y; y <= bottomRight.Y; ++y)
				{
					var splatPos = new Point(x, y);
					var terrainPos = Scene.Terrain.ToTerrainPosition(splatPos);
					var dist = Vector2.Distance(new Vector2(markerPos.X, markerPos.Z), terrainPos);

					if (dist > radius)
					{
						continue;
					}

					switch (Instrument.Type)
					{
						case InstrumentType.PaintTexture1:
							UpdateTerrainSplatMap(splatPos, SplatManChannel.First, power);
							break;
						case InstrumentType.PaintTexture2:
							UpdateTerrainSplatMap(splatPos, SplatManChannel.Second, power);
							break;
						case InstrumentType.PaintTexture3:
							UpdateTerrainSplatMap(splatPos, SplatManChannel.Third, power);
							break;
						case InstrumentType.PaintTexture4:
							UpdateTerrainSplatMap(splatPos, SplatManChannel.Fourth, power);
							break;
					}
				}
			}
		}

		private void ApplyInstrument()
		{
			if (Scene == null || Scene.Terrain == null || Scene.Marker.Position == null)
			{
				return;
			}

			var mouse = Mouse.GetState();
			if (mouse.LeftButton != ButtonState.Pressed || Instrument.Type == InstrumentType.None || Instrument.Power.EpsilonEquals(0.0f))
			{
				return;
			}

			if (Instrument.Type == InstrumentType.Water)
			{
			}
			else if (Instrument.Type == InstrumentType.RaiseTerrain || Instrument.Type == InstrumentType.LowerTerrain)
			{
				ApplyLowerRaise();
			}
			else
			{
				ApplyPaint();
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

			if (Instrument.Type == InstrumentType.Water)
			{
				if (IsMouseLeftButtonDown && Scene.Marker.Position != null)
				{
					var markerPos = Scene.Marker.Position.Value;
					_touchDownStart = markerPos;
				}
			}
			else
			{
				ApplyInstrument();
			}

			return result;
		}

		private void GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ)
		{
			var markerPos = Scene.Marker.Position.Value;

			startPos = new Vector3(Math.Min(markerPos.X, _touchDownStart.Value.X),
				Scene.DefaultWaterLevel,
				Math.Min(markerPos.Z, _touchDownStart.Value.Z));

			sizeX = Math.Abs(markerPos.X - _touchDownStart.Value.X);
			sizeZ = Math.Abs(markerPos.Z - _touchDownStart.Value.Z);
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

			if (Instrument.Type == InstrumentType.Water)
			{
				if (_touchDownStart != null && Scene.Marker.Position != null)
				{
					GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ);
					if (sizeX > 0 && sizeZ > 0)
					{
						if (_waterMarker == null)
						{
							var mesh = new Mesh(PrimitiveMeshes.SquarePositionFromZeroToOne, Material.CreateSolidMaterial(Color.Green));
							_waterMarker = new NursiaModel(mesh).CreateInstance();
						}

						if (!Scene.Models.Contains(_waterMarker)) 
						{
							Scene.Models.Add(_waterMarker);
						}

						_waterMarker.Transform = Matrix.CreateScale(sizeX, 0, sizeZ) * Matrix.CreateTranslation(startPos);
					}
				}
			}
			else
			{
				ApplyInstrument();
			}
		}
	}
}
