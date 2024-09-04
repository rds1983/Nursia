using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.UI;
using Nursia;
using Nursia.Rendering;
using Nursia.Modelling;
using System.Collections.Generic;
using VertexPosition = Nursia.Rendering.Vertices.VertexPosition;
using Nursia.Utilities;
using static Nursia.Utilities.CameraInputController;
using Nursia.Rendering.Lights;

namespace NursiaEditor.UI
{
	public class SceneWidget : Widget
	{
		private const int GridSize = 200;

		private Scene _scene;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private CameraInputController _controller;
		private MeshNode _gridMesh;
		private Nursia.Modelling.ModelMesh _waterMarker;
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

		private MeshNode GridMesh
		{
			get
			{
				if (_gridMesh == null)
				{
					var vertices = new List<VertexPosition>();
					var indices = new List<short>();

					short idx = 0;
					for (var x = -GridSize; x <= GridSize; ++x)
					{
						vertices.Add(new VertexPosition
						{
							Position = new Vector3(x, 0, -GridSize)
						});

						vertices.Add(new VertexPosition
						{
							Position = new Vector3(x, 0, GridSize)
						});

						indices.Add(idx);
						++idx;
						indices.Add(idx);
						++idx;
					}

					for (var z = -GridSize; z <= GridSize; ++z)
					{
						vertices.Add(new VertexPosition
						{
							Position = new Vector3(-GridSize, 0, z)
						});

						vertices.Add(new VertexPosition
						{
							Position = new Vector3(GridSize, 0, z)
						});

						indices.Add(idx);
						++idx;
						indices.Add(idx);
						++idx;
					}

					var mesh = new Mesh(vertices.ToArray(), indices.ToArray(), PrimitiveType.LineList);

					_gridMesh = new MeshNode
					{
						Mesh = mesh,
						Material = new ColorMaterial
						{
							Color = Color.Green,
						},
					};
				}

				return _gridMesh;
			}
		}


		private static bool IsMouseLeftButtonDown
		{
			get
			{
				var mouseState = Mouse.GetState();
				return (mouseState.LeftButton == ButtonState.Pressed);
			}
		}

		public SceneWidget()
		{
			ClipToBounds = true;
		}

		/*		private Vector3? CalculateMarkerPosition()
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
					var bb = MathUtils.CreateBoundingBox(0, Scene.Terrain.Size.X, 0, 0, 0, Scene.Terrain.Size.Y);
					var intersectDist = ray.Intersects(bb);
					if (intersectDist == null)
					{
						return null;
					}

					var markerPosition = nearPoint + direction * intersectDist.Value;

					// Now determine where we intersect terrain rectangle with real height
					var height = Scene.Terrain.GetHeight(markerPosition.X, markerPosition.Z);
					bb = MathUtils.CreateBoundingBox(0, Scene.Terrain.Size.X, height, height, 0, Scene.Terrain.Size.Y);
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
				}*/

		private void UpdateKeyboard()
		{
			var keyboardState = Keyboard.GetState();

			// Manage camera input controller
			_controller.SetControlKeyState(ControlKeys.Left, keyboardState.IsKeyDown(Keys.A));
			_controller.SetControlKeyState(ControlKeys.Right, keyboardState.IsKeyDown(Keys.D));
			_controller.SetControlKeyState(ControlKeys.Forward, keyboardState.IsKeyDown(Keys.W));
			_controller.SetControlKeyState(ControlKeys.Backward, keyboardState.IsKeyDown(Keys.S));
			_controller.SetControlKeyState(ControlKeys.Up, keyboardState.IsKeyDown(Keys.Up));
			_controller.SetControlKeyState(ControlKeys.Down, keyboardState.IsKeyDown(Keys.Down));
			_controller.Update();
		}

		public override void InternalRender(Myra.Graphics2D.RenderContext context)
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

				_renderer.Render(Scene, Scene.Camera);
				_renderer.Render(GridMesh, Scene.Camera);

				//				UpdateMarker();
				/*				_renderer.Begin();
								_renderer.DrawScene(Scene);

								if (_waterMarker != null)
								{
									_renderer.DrawMesh(_waterMarker, Scene.Camera);
								}

								if (_modelMarker != null)
								{
									_renderer.DrawModel(_modelMarker, Scene.Camera);
								}

								_renderer.End();*/
			}
			catch (Exception ex)
			{
				var msg = Dialog.CreateMessageBox("Error", ex.Message);
				msg.ShowModal(Desktop);
			}
			finally
			{
				device.Viewport = oldViewport;
			}

			var camera = Scene.Camera;
			var widgetViewport = new Viewport(0, 0, ActualBounds.Width, ActualBounds.Height);
			var projection = camera.CalculateProjection(widgetViewport.AspectRatio);

			// Draw lights' icons'
			Scene.Iterate(n =>
			{
				var asLight = n as BaseLight;
				if (asLight != null)
				{
					var p = widgetViewport.Project(asLight.Translation,
						projection, camera.View, Matrix.Identity);

					var icon = NursiaEditor.Resources.IconDirectionalLight;

					p.X -= icon.Width / 2;
					p.Y -= icon.Height / 2;
					context.Draw(icon, new Vector2(p.X, p.Y), Color.White);
				}
			});
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

			/*			if (Instrument.Type == InstrumentType.Water && _touchDownStart != null && Scene.Marker.Position != null)
						{
							GetWaterMarkerPos(out Vector3 startPos, out float sizeX, out float sizeZ);

							if (sizeX > 0 && sizeZ > 0)
							{
								var waterTile = new WaterTile(startPos.X, startPos.Z, Scene.DefaultWaterLevel, sizeX, sizeZ);
								Scene.WaterTiles.Add(waterTile);
							}

							_touchDownStart = null;
							_waterMarker = null;
						}*/
		}

		/*		private void UpdateTerrainHeight(Point pos, float power)
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
				}*/

		public override void OnMouseMoved()
		{
			base.OnMouseMoved();

			var mouseState = Mouse.GetState();
			_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));

			/*if (Instrument.Type == InstrumentType.Model)
			{
				if (Scene.Marker.Position != null)
				{
					if (_modelMarker == null || _modelMarker != Instrument.Model)
					{
						_modelMarker = Instrument.Model;
					}

					var pos = Scene.Marker.Position.Value;
					pos.Y = -_modelMarker.BoundingBox.Min.Y;
					pos.Y += Scene.Terrain.GetHeight(pos.X, pos.Z);

					_modelMarker.Transform = Matrix.CreateTranslation(pos);
				} else
				{
					_modelMarker = null;
				}
			}*/
		}

		public override void OnMouseLeft()
		{
			base.OnMouseLeft();

			_modelMarker = null;
		}

		/*		public override void OnTouchDown()
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

						var model = Instrument.Model;
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
				}*/

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

			/*if (!IsMouseLeftButtonDown || Scene.Marker.Position == null)
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
			}*/
		}
	}
}
