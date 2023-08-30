using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.Properties;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Utils;
using Nursia.Utilities;
using NursiaStudio.UI;
using System.IO;

namespace NursiaStudio
{
	public class StudioGame : Game
	{
		private static StudioGame _instance;

		private readonly GraphicsDeviceManager _graphics;
		private CameraInputController _controller;
		private Desktop _desktop = null;
		private MainForm _mainForm;
		private PropertyGrid _propertyGrid;
		private SpriteBatch _spriteBatch;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly Scene _scene = new Scene();
		private readonly ForwardRenderer _renderer = new ForwardRenderer();

		public static Scene Scene
		{
			get
			{
				return _instance._scene;
			}
		}

		public StudioGame()
		{
			_instance = this;

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// UI
			MyraEnvironment.Game = this;
			_mainForm = new MainForm();

			_propertyGrid = new PropertyGrid();
			_mainForm._panelProperties.Widgets.Add(_propertyGrid);

			_mainForm._listExplorer.SelectedIndexChanged += _listExplorer_SelectedIndexChanged;

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			// Nursia
			Nrs.Game = this;

			// Light
			_scene.Lights.Add(new DirectLight
			{
				Color = Color.White,
				Position = new Vector3(10000, 10000, -10000),
				Direction = new Vector3(0, -1, 0)
			});

			// Water
			_scene.WaterTiles.Add(new WaterTile(0, 0, 0, 100));

			// Set camera
			_scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);

			_controller = new CameraInputController(_scene.Camera);

			RefreshExplorer();
		}

		private void _listExplorer_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			var list = _mainForm._listExplorer;
			_propertyGrid.Object = list.SelectedItem.Tag;
		}

		private void RefreshExplorer()
		{
			var list = _mainForm._listExplorer;
			list.Items.Clear();

			// Lights
			foreach(var light in _scene.Lights)
			{
				list.Items.Add(new ListItem
				{
					Text = "Directional Light",
					Tag = light
				});
			}

			// Skybox
			if (_scene.Skybox != null)
			{
				list.Items.Add(new ListItem
				{
					Text = "Skybox",
					Tag = _scene.Skybox
				});
			}

			// Water
			foreach(var water in _scene.WaterTiles)
			{
				list.Items.Add(new ListItem
				{
					Text = "Water",
					Tag = water
				});
			}
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_fpsCounter.Update(gameTime);

			var keyboardState = Keyboard.GetState();

			// Manage camera input controller
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Left, keyboardState.IsKeyDown(Keys.A));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Right, keyboardState.IsKeyDown(Keys.D));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Forward, keyboardState.IsKeyDown(Keys.W));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Backward, keyboardState.IsKeyDown(Keys.S));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Up, keyboardState.IsKeyDown(Keys.Up));
			_controller.SetControlKeyState(CameraInputController.ControlKeys.Down, keyboardState.IsKeyDown(Keys.Down));

			var mouseState = Mouse.GetState();
			_controller.SetTouchState(CameraInputController.TouchType.Move, mouseState.LeftButton == ButtonState.Pressed);
			_controller.SetTouchState(CameraInputController.TouchType.Rotate, mouseState.RightButton == ButtonState.Pressed);

			_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));

			_controller.Update();
		}

		private void DrawScene()
		{
			var device = GraphicsDevice;
			var oldViewport = device.Viewport;
			var bounds = _mainForm._panelScene.ActualBounds;

			try
			{
				device.Viewport = new Viewport(bounds.X, bounds.Y, bounds.Width, bounds.Height);

				_renderer.Begin();
				_renderer.DrawScene(_scene);
				_renderer.End();
			}
			finally
			{
				device.Viewport = oldViewport;
			}
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawScene();

			_mainForm._labelCamera.Text = "Camera: " + _scene.Camera.ToString();
			_mainForm._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;
			_mainForm._labelMeshes.Text = "Meshes: " + _renderer.Statistics.MeshesDrawn;

			_desktop.Render();

/*			_spriteBatch.Begin();

			_spriteBatch.Draw(_renderer.WaterReflection, 
				new Rectangle(0, 500, 600, 300), 
				Color.White);

			_spriteBatch.End();*/

			_fpsCounter.Draw(gameTime);
		}
	}
}