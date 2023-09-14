using AssetManagementBase;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Samples.LevelEditor.UI;
using StbImageSharp;

namespace Nursia.Samples.LevelEditor
{
	public class StudioGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Desktop _desktop = null;
		private MainForm _mainForm;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();

		public Scene Scene
		{
			get => _mainForm.Scene;
			set => _mainForm.Scene = value;
		}

		public ForwardRenderer Renderer { get => _mainForm.Renderer; }
		private SpriteBatch _spriteBatch;

		public StudioGame()
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
			using (var stream = assetManager.OpenAssetStream("../../skybox/" + name))
			{
				image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
			}

			return image.Data;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// UI
			MyraEnvironment.Game = this;
			Nrs.Game = this;
			_mainForm = new MainForm();


			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			var baseFolder = @"D:\Temp\Nursia\scenes\scene1";
			var assetManager = AssetManager.CreateFileAssetManager(baseFolder);
			var scene = Scene.Load(Path.Combine(baseFolder, @"scene.json"), assetManager);

			scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
			_mainForm.Scene = scene;

			_mainForm.BasePath = baseFolder;
			_mainForm.AssetManager = assetManager;

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

			scene.Skybox = new Skybox(100)
			{
				Texture = texture
			};

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			var assetFolder = Path.Combine(Utils.ExecutingAssemblyDirectory, "Assets");
			ModelStorage.Load(Path.Combine(assetFolder, "models"));
			_mainForm.RefreshLibrary();
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_fpsCounter.Update(gameTime);
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

			_spriteBatch.Begin();
			var tex = Renderer.WaterRefraction;
			// _spriteBatch.Draw(tex, new Rectangle(0, 0, tex.Width, tex.Height), Color.White);
			var tex2 = Renderer.WaterReflection;
			// _spriteBatch.Draw(tex2, new Rectangle(0, 0, tex2.Width, tex2.Height), Color.White);
			_spriteBatch.End();
		}
	}
}