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
using System.IO;

namespace SampleScene
{
	public class SampleGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private NursiaModel _model;
		private CameraInputController _controller;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly Scene _scene = new Scene();
		private Desktop _desktop;

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

		private NursiaModel LoadModel(string file)
		{
			var result = NursiaModel.LoadFromGltf(file);

			_scene.Models.Add(result);

			// Reset camera
			_scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);

			return result;
		}

		private Texture2D LoadTexture(string path)
		{
			using (var stream = File.OpenRead(path))
			{
				return Texture2D.FromStream(GraphicsDevice, stream);
			}
		}

		private ImageResult LoadImage(string path)
		{
			using (var stream = File.OpenRead(path))
			{
				return ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			}
		}

		private void LoadColors(string path, out byte[] data)
		{
			var image = LoadImage(path);

			data = image.Data;
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

			var folder = @"D:\Projects\Nursia\sampleContent";

			// Model
			_model = LoadModel(@"D:\Temp\Sinbad\Sinbad.glb");
			_model.Transform = Matrix.CreateTranslation(new Vector3(0, 10, 0));

			// Terrain
			var grassy = LoadTexture(Path.Combine(folder, @"terrain\grassy2.png"));
			_scene.Terrain = new Terrain(400);

			// Generate height
			var generator = new HeightMapGenerator();
			GenerationConfig.Instance.WorldSize = (int)_scene.Terrain.Size;
			var heightMap = generator.Generate();

			_scene.Terrain.HeightFunc = (x, z) =>
			{
				if (x < 0)
				{
					x = 0;
				}

				if (x >= heightMap.GetLength(0))
				{
					x = heightMap.GetLength(0) - 1;
				}

				if (z < 0)
				{
					z = 0;
				}

				if (z >= heightMap.GetLength(1))
				{
					z = heightMap.GetLength(1) - 1;
				}

				var result = (heightMap[(int)x, (int)z] * 100) - 50;

				return result;

/*				int r = (int)(x / 100) + (int)(z / 100);

				return r % 2 == 0 ? -10 : 10;*/
			};

			_scene.Terrain.SetTexture(grassy);

			// Water
			_scene.WaterTiles.Add(new WaterTile(0, 0, 0, _scene.Terrain.Size));

			// Skybox
			var skyboxFolder = Path.Combine(folder, "skybox");
			var texture = new TextureCube(GraphicsDevice, 1024,
				false, SurfaceFormat.Color);
			byte[] data = null;
			LoadColors(Path.Combine(skyboxFolder,  @"negX.png"), out data);
			texture.SetData(CubeMapFace.NegativeX, data);
			LoadColors(Path.Combine(skyboxFolder, @"negY.png"), out data);
			texture.SetData(CubeMapFace.NegativeY, data);
			LoadColors(Path.Combine(skyboxFolder, @"negZ.png"), out data);
			texture.SetData(CubeMapFace.NegativeZ, data);
			LoadColors(Path.Combine(skyboxFolder, @"posX.png"), out data);
			texture.SetData(CubeMapFace.PositiveX, data);
			LoadColors(Path.Combine(skyboxFolder, @"posY.png"), out data);
			texture.SetData(CubeMapFace.PositiveY, data);
			LoadColors(Path.Combine(skyboxFolder, @"posZ.png"), out data);
			texture.SetData(CubeMapFace.PositiveZ, data);

			_scene.Skybox = new Skybox(100)
			{
				Texture = texture
			};

			_scene.Lights.Add(new DirectLight
			{
				Color = Color.White,
				Position = new Vector3(10000, 10000, -10000),
				Direction = new Vector3(0, -1, 0)
			});

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

/*			_spriteBatch.Begin();

			_spriteBatch.Draw(_renderer.WaterReflection, 
				new Rectangle(0, 500, 600, 300), 
				Color.White);

			_spriteBatch.End();*/

			_fpsCounter.Draw(gameTime);
		}
	}
}