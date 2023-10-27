using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra;
using Myra.Graphics2D.UI;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Landscape;
using Nursia.Graphics3D.Lights;
using Nursia.Graphics3D.Modelling;
using Nursia.Graphics3D.Utils;
using SampleScene.UI;
using StbImageSharp;
using System;
using System.IO;

namespace SampleScene
{
	public class SampleGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private ModelInstance _model;
		private CameraInputController _controller;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly Scene _scene = new Scene();
		private Desktop _desktop;
		private float _passed;
		private DateTime? _animationMoment;

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

		private byte[] LoadSkyboxImage(AssetManager assetManager, string name)
		{
			ImageResult image;
			using (var stream = assetManager.Open("skybox/" + name))
			{
				image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			}

			return image.Data;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();

			_desktop = new Desktop();
			_desktop.Root = _mainPanel;

			// Nursia
			Nrs.Game = this;

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(Utils.ExecutingAssemblyDirectory, "Assets"));

			// Model
			_model = assetManager.LoadGltf("models/Sinbad.glb").CreateInstance();
			_model.Transform = Matrix.CreateTranslation(new Vector3(0, 10, 0));
			_model.CurrentAnimation = _model.Model.Animations["Dance"];
			_scene.Models.Add(_model);

			// Terrain
			var grassy = assetManager.LoadTexture2D(GraphicsDevice, @"terrain/grassy2.png");
/*			_scene.Terrain = new Terrain
			{
				TextureBase = grassy
			};*/

			// Water
			_scene.WaterTiles.Add(new WaterTile(0, 0, 0, 1000, 1000));

			// Skybox
			var texture = new TextureCube(GraphicsDevice, 1024, false, SurfaceFormat.Color);

			var data = LoadSkyboxImage(assetManager, @"negX.png");
			texture.SetData(CubeMapFace.NegativeX, data);
			data = LoadSkyboxImage(assetManager, @"negY.png");
			texture.SetData(CubeMapFace.NegativeY, data);
			data = LoadSkyboxImage(assetManager, @"negZ.png");
			texture.SetData(CubeMapFace.NegativeZ, data);
			data = LoadSkyboxImage(assetManager, @"posX.png");
			texture.SetData(CubeMapFace.PositiveX, data);
			data = LoadSkyboxImage(assetManager, @"posY.png");
			texture.SetData(CubeMapFace.PositiveY, data);
			data = LoadSkyboxImage(assetManager, @"posZ.png");
			texture.SetData(CubeMapFace.PositiveZ, data);

			_scene.Skybox = new Skybox(100)
			{
				Texture = texture
			};

			_scene.DirectLights.Add(new DirectLight
			{
				Color = Color.White,
				Position = new Vector3(10000, 10000, -10000),
				Direction = new Vector3(0, -1, 0)
			});

			// Reset camera
			_scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);

			_controller = new CameraInputController(_scene.Camera);
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
			if (_animationMoment != null)
			{
				foreach (var model in _scene.Models)
				{
					if (_animationMoment != null)
					{
						var passed = (float)(DateTime.Now - _animationMoment.Value).TotalSeconds;

						_passed += passed;

						if (_passed > model.CurrentAnimation.Time)
						{
							_passed = 0;
						}

						model.UpdateCurrentAnimation(_passed);
					}
				}
			}
			_animationMoment = DateTime.Now;

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
			_spriteBatch.End();

			_fpsCounter.Draw(gameTime);
		}
	}
}