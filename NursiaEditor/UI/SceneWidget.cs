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
		private Mesh _waterMarker;
		private ModelInstance _modelMarker;
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
			var bb = Utils.CreateBoundingBox(0, Scene.Terrain.Size.X, 0, 0, 0, Scene.Terrain.Size.Y);
			var intersectDist = ray.Intersects(bb);
			if (intersectDist == null)
			{
				return null;
			}

			var markerPosition = nearPoint + direction * intersectDist.Value;

			// Now determine where we intersect terrain rectangle with real height
			var height = Scene.Terrain.GetHeight(markerPosition.X, markerPosition.Z);
			bb = Utils.CreateBoundingBox(0, Scene.Terrain.Size.X, height, height, 0, Scene.Terrain.Size.Y);
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

				if (_waterMarker != null)
				{
					_renderer.DrawMesh(_waterMarker, Scene.Camera);
				}

				if (_modelMarker != null)
				{
					_renderer.DrawModel(_modelMarker, Scene.Camera);
				}

				_renderer.End();
			}
			catch(SharpDX.CompilationException ex)
			{
				var msg = Dialog.CreateMessageBox("Error", ex.Message);
				msg.ShowModal(Desktop);
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
				_waterMarker = null;
			}
		}

		private void UpdateTerrainHeight(Point pos, float power)
		{
			var height = Scene.Terrain.GetHeightByHeightPos(pos);
			height += power;
			Scene.Terrain.SetHeightByHeightPos(pos, height);
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

			var topLeft = Scene.Terrain.ToHeightPosition(markerPos.X - radius, markerPos.Z - radius);
			var bottomRight = Scene.Terrain.ToHeightPosition(markerPos.X + radius, markerPos.Z + radius);

			for (var x = topLeft.X; x <= bottomRight.X; ++x)
			{
				for (var y = topLeft.Y; y <= bottomRight.Y; ++y)
				{
					var heightPos = new Point(x, y);
					var terrainPos = Scene.Terrain.HeightToTerrainPosition(heightPos);
					var dist = Vector2.Distance(new Vector2(markerPos.X, markerPos.Z), terrainPos);

					if (dist > radius)
					{
						continue;
					}

					switch (Instrument.Type)
					{
						case InstrumentType.None:
							break;
						case InstrumentType.RaiseTerrain:
							UpdateTerrainHeight(heightPos, power);
							break;
						case InstrumentType.LowerTerrain:
							UpdateTerrainHeight(heightPos, -power);
							break;
					}
				}
			}
		}

		private void ApplyTerrainPaint()
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
					var terrainPos = Scene.Terrain.SplatToTerrainPosition(splatPos);
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

		private void ApplyPaintInstrument()
		{
			if (Instrument.Type == InstrumentType.RaiseTerrain || Instrument.Type == InstrumentType.LowerTerrain)
			{
				ApplyLowerRaise();
			}
			else
			{
				ApplyTerrainPaint();
			}
		}

		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			var mouseState = Mouse.GetState();
			_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));

			if (Instrument.Type == InstrumentType.Model)
			{
				if (Scene.Marker.Position != null)
				{
					if (_modelMarker == null || _modelMarker.Model != Instrument.Model)
					{
						_modelMarker = Instrument.Model.CreateInstance();
					}

					var pos = Scene.Marker.Position.Value;
					pos.Y = -_modelMarker.BoundingBox.Min.Y;
					pos.Y += Scene.Terrain.GetHeight(pos.X, pos.Z);

					_modelMarker.Transform = Matrix.CreateTranslation(pos);
				} else
				{
					_modelMarker = null;
				}
			}
		}

		public override void OnMouseLeft()
		{
			base.OnMouseLeft();

			_modelMarker = null;
		}

		public override void OnTouchDown()
		{
			base.OnTouchDown();

			if (!IsMouseLeftButtonDown || Scene.Marker.Position == null)
			{
				return;
			}

			if (Instrument.IsPaintInstrument)
			{
				ApplyPaintInstrument();
			}
			else if (Instrument.Type == InstrumentType.Water)
			{
				_touchDownStart = Scene.Marker.Position.Value;
			}
			else if (Instrument.Type == InstrumentType.Model)
			{
				var pos = Scene.Marker.Position.Value;

				var model = Instrument.Model.CreateInstance();
				pos.Y = -model.BoundingBox.Min.Y;
				pos.Y += Scene.Terrain.GetHeight(pos.X, pos.Z);

				model.Transform = Matrix.CreateTranslation(pos);

				Scene.Models.Add(model);
			}
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

			if (!IsMouseLeftButtonDown || Scene.Marker.Position == null)
			{
				return;
			}

			if (Instrument.IsPaintInstrument)
			{
				ApplyPaintInstrument();
			}
			else if (Instrument.Type == InstrumentType.Water)
			{
				if (_touchDownStart != null)
				{
					GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ);
					if (sizeX > 0 && sizeZ > 0)
					{
						if (_waterMarker == null)
						{
							_waterMarker = new Mesh(PrimitiveMeshes.SquarePositionFromZeroToOne, Material.CreateSolidMaterial(Color.Green));
						}

						_waterMarker.Transform = Matrix.CreateScale(sizeX, 0, sizeZ) * Matrix.CreateTranslation(startPos);
					}
				}
			}
		}
	}
}
