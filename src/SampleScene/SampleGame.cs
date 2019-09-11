using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModelViewer.UI;
using Myra;
using Myra.Graphics2D.UI;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using Nursia.Graphics3D.Utils;
using Nursia.Graphics3D.Water;
using System;
using System.Collections.Generic;
using System.IO;

namespace ModelViewer
{
	public class SampleGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Sprite3D _model;
		private CameraInputController _controller;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private Desktop _desktop = null;
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private static readonly List<DirectLight> _defaultLights = new List<DirectLight>();
		private readonly Scene _scene = new Scene();

		static SampleGame()
		{
			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f),
				Color = new Color(1, 0.9607844f, 0.8078432f)
			});

			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f),
				Color = new Color(0.9647059f, 0.7607844f, 0.4078432f)
			});

			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f),
				Color = new Color(0.3231373f, 0.3607844f, 0.3937255f)
			});
		}

		public SampleGame()
		{
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

		private Sprite3D LoadModel(string file)
		{
			var folder = Path.GetDirectoryName(file);
			var data = File.ReadAllText(file);
			var result = Sprite3D.LoadFromJson(data,
					n =>
					{
						using (var stream = File.OpenRead(Path.Combine(folder, n)))
						{
							return Texture2D.FromStream(GraphicsDevice, stream);
						}
					});

			_scene.Models.Add(result);

			// Reset camera
			_scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);

			return result;
		}

		private void LoadImage(string path, ref Color[] data)
		{
			Texture2D texture;
			using (var stream = File.OpenRead(path))
			{
				texture = Texture2D.FromStream(GraphicsDevice, stream);
			}

			var size = new Point(texture.Width, texture.Height);
			if (data == null || data.Length != size.X * size.Y)
			{
				data = new Color[size.X * size.Y];
			}
			texture.GetData(data);
			texture.Dispose();
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();

			_mainPanel._checkLightning.PressedChanged += _checkLightning_PressedChanged;

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainPanel);

			// Nursia
			Nrs.Game = this;

			_scene.WaterTiles.Add(new WaterTile(0, 0, -10));

			_model = LoadModel(@"D:\Projects\Nursia\samples\models\knight.g3dj");
			_model.CurrentAnimation = _model.Animations["Attack"];

			var texture = new TextureCube(GraphicsDevice, 1024,
				false, SurfaceFormat.Color);
			Color[] data = null;
			LoadImage(@"D:\Projects\Nursia\samples\skybox\negX.png", ref data);
			texture.SetData(CubeMapFace.NegativeX, data);
			LoadImage(@"D:\Projects\Nursia\samples\skybox\negY.png", ref data);
			texture.SetData(CubeMapFace.NegativeY, data);
			LoadImage(@"D:\Projects\Nursia\samples\skybox\negZ.png", ref data);
			texture.SetData(CubeMapFace.NegativeZ, data);
			LoadImage(@"D:\Projects\Nursia\samples\skybox\posX.png", ref data);
			texture.SetData(CubeMapFace.PositiveX, data);
			LoadImage(@"D:\Projects\Nursia\samples\skybox\posY.png", ref data);
			texture.SetData(CubeMapFace.PositiveY, data);
			LoadImage(@"D:\Projects\Nursia\samples\skybox\posZ.png", ref data);
			texture.SetData(CubeMapFace.PositiveZ, data);

			_scene.Skybox = new Skybox(100)
			{
				Texture = texture
			};

			_controller = new CameraInputController(_scene.Camera);
		}

		private void _checkLightning_PressedChanged(object sender, EventArgs e)
		{
			_scene.Lights.Clear();
			if (_mainPanel._checkLightning.IsPressed)
			{
				_scene.Lights.AddRange(_defaultLights);
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

		private void DrawModel()
		{
			foreach(var model in _scene.Models)
			{
				model.UpdateCurrentAnimation();
			}

			_renderer.Begin();
			_renderer.DrawScene(_scene);
			_renderer.End();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawModel();

			_mainPanel._labelCamera.Text = "Camera: " + _scene.Camera.ToString();
			_mainPanel._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;
			_mainPanel._labelMeshes.Text = "Meshes: " + _renderer.Statistics.MeshesDrawn;

			_desktop.Render();

			_spriteBatch.Begin();

			_spriteBatch.Draw(_renderer.WaterReflection, 
				new Rectangle(0, 500, 600, 300), 
				Color.White);

			_spriteBatch.End();

			_fpsCounter.Draw(gameTime);
		}
	}
}