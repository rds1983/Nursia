using AssetManagementBase;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Utils;
using Nursia.Samples.LevelEditor.UI;
using Nursia.Graphics3D.Landscape;

namespace Nursia.Samples.LevelEditor
{
	public class StudioGame : Game
	{
		private static StudioGame _instance;

		private readonly GraphicsDeviceManager _graphics;
		private CameraInputController _controller;
		private Desktop _desktop = null;
		private MainForm _mainForm;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();

		public Scene Scene
		{
			get => _mainForm.Scene;
			set => _mainForm.Scene = value;
		}

		public ForwardRenderer Renderer { get => _mainForm.Renderer; }

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

			// UI
			MyraEnvironment.Game = this;
			Nrs.Game = this;
			_mainForm = new MainForm();

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(Utils.ExecutingAssemblyDirectory, "Assets"));

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			// Light
			var scene = new Scene
			{
				Terrain = new Terrain
				{
					Texture = assetManager.LoadTexture2D(GraphicsDevice, @"terrain/grassy2.png")
				}
			};
			scene.Lights.Add(new DirectLight
			{
				Color = Color.White,
				Position = new Vector3(10000, 10000, -10000),
				Direction = new Vector3(0, -1, 0)
			});

			scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
			_mainForm.Scene = scene;

			_controller = new CameraInputController(scene.Camera);
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

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_mainForm._labelCamera.Text = "Camera: " + Scene.Camera.ToString();
			_mainForm._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;
			_mainForm._labelMeshes.Text = "Meshes: " + Renderer.Statistics.MeshesDrawn;

			_desktop.Render();

			_fpsCounter.Draw(gameTime);
		}
	}
}