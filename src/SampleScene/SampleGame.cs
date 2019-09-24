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
using Nursia.Utilities;
using SampleScene.UI;
using System.IO;

namespace SampleScene
{
	public class SampleGame : Game
	{
		private const int HeightImageSize = 128;

		private readonly GraphicsDeviceManager _graphics;
		private NursiaModel _model;
		private CameraInputController _controller;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private Desktop _desktop = null;
		private MainPanel _mainPanel;
		private SpriteBatch _spriteBatch;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly Scene _scene = new Scene();

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
			var folder = Path.GetDirectoryName(file);
			var data = File.ReadAllText(file);
			var result = NursiaModel.LoadFromJson(data,
					n => LoadTexture(Path.Combine(folder, n)));

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

		private Image2D LoadImage(string path)
		{
			using (var stream = File.OpenRead(path))
			{
				return Image2D.FromStream(stream);
			}
		}

		private void LoadColors(string path, ref Color[] data)
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
			_desktop.Widgets.Add(_mainPanel);

			// Nursia
			Nrs.Game = this;

			var folder = @"D:\Projects\Nursia\samples";

			// Model
			_model = LoadModel(Path.Combine(folder, @"models\knight.g3dj"));
			_model.Transform = Matrix.CreateTranslation(new Vector3(0, 10, 0));
			_model.CurrentAnimation = _model.Animations["Attack"];

			// Terrain
			var grassy = LoadTexture(Path.Combine(folder, @"terrain\grassy2.png"));
			_scene.Terrain = new Terrain(8, 8);

			// Generate height
			var generator = new HeightMapGenerator();
			GenerationConfig.Instance.WorldSize = _scene.Terrain.TilesPerX * HeightImageSize;
			var heightMap = generator.Generate();
			var image = new Image2D(HeightImageSize, HeightImageSize);
			for (var x = 0; x < _scene.Terrain.TilesPerX; ++x)
			{
				for (var z = 0; z < _scene.Terrain.TilesPerZ; ++z)
				{
					for(var i = 0; i < HeightImageSize; ++i)
					{
						for(var j = 0; j < HeightImageSize; ++j)
						{
							var c = (byte)(255 * heightMap[x * HeightImageSize + j,
								z * HeightImageSize + i]);
							image[j, i] = new Color(c, c, c);
						}
					}
					var terrainTile = _scene.Terrain[x, z];
					terrainTile.HeightMap = new HeightMap(image);
					terrainTile.Texture = grassy;
				}
			}

			// Water
			_scene.WaterTiles.Add(new WaterTile(0, 0, -10, _scene.Terrain.TotalSizeX));

			// Skybox
			var skyboxFolder = Path.Combine(folder, "skybox");
			var texture = new TextureCube(GraphicsDevice, 1024,
				false, SurfaceFormat.Color);
			Color[] data = null;
			LoadColors(Path.Combine(skyboxFolder,  @"negX.png"), ref data);
			texture.SetData(CubeMapFace.NegativeX, data);
			LoadColors(Path.Combine(skyboxFolder, @"negY.png"), ref data);
			texture.SetData(CubeMapFace.NegativeY, data);
			LoadColors(Path.Combine(skyboxFolder, @"negZ.png"), ref data);
			texture.SetData(CubeMapFace.NegativeZ, data);
			LoadColors(Path.Combine(skyboxFolder, @"posX.png"), ref data);
			texture.SetData(CubeMapFace.PositiveX, data);
			LoadColors(Path.Combine(skyboxFolder, @"posY.png"), ref data);
			texture.SetData(CubeMapFace.PositiveY, data);
			LoadColors(Path.Combine(skyboxFolder, @"posZ.png"), ref data);
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