using AssetManagementBase;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia.Graphics3D.Lights;
using Nursia.Samples.LevelEditor.UI;
using Nursia.Graphics3D.Landscape;

namespace Nursia.Samples.LevelEditor
{
	public class StudioGame : Game
	{
		private static StudioGame _instance;

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

		private void LoadTerrainTexture(Terrain terrain, int index, AssetManager assetManager, string filePath)
		{
			var texture = assetManager.LoadTexture2D(GraphicsDevice, filePath);
			var name = Path.GetFileName(filePath);

			switch(index)
			{
				case 1:
					terrain.TexturePaint1 = texture;
					terrain.TexturePaintName1 = name;
					break;
				case 2:
					terrain.TexturePaint2 = texture;
					terrain.TexturePaintName2 = name;
					break;
				case 3:
					terrain.TexturePaint3 = texture;
					terrain.TexturePaintName3 = name;
					break;
				case 4:
					terrain.TexturePaint4 = texture;
					terrain.TexturePaintName4 = name;
					break;
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
					TextureBase = assetManager.LoadTexture2D(GraphicsDevice, @"terrain/grassy2.png"),
				}
			};

			LoadTerrainTexture(scene.Terrain, 1, assetManager, @"terrain/dirt.png");
			LoadTerrainTexture(scene.Terrain, 2, assetManager, @"terrain/mud.png");
			LoadTerrainTexture(scene.Terrain, 3, assetManager, @"terrain/path.png");
			LoadTerrainTexture(scene.Terrain, 4, assetManager, @"terrain/pinkFlowers.png");

			scene.DirectLights.Add(new DirectLight
			{
				Color = Color.White,
				Position = new Vector3(10000, 10000, -10000),
				Direction = new Vector3(1, -1, -1)
			});

			scene.Camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
			_mainForm.Scene = scene;

			_spriteBatch = new SpriteBatch(GraphicsDevice);
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
			_spriteBatch.Draw(Scene.Terrain.GetTile(0, 0).SplatTexture, new Rectangle(0, 0, 100, 100), Color.White);
			_spriteBatch.End();
		}
	}
}