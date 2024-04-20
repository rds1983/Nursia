using AssetManagementBase;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Graphics3D;
using Nursia.Graphics3D.ForwardRendering;
using Nursia;
using NursiaEditor.UI;
using Myra.Graphics2D.UI.ColorPicker;
using System;
using System.Linq;

namespace NursiaEditor
{
	public class StudioGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Desktop _desktop = null;
		private MainForm _mainForm;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly State _state;

		public static StudioGame Instance { get; private set; }

		public Scene Scene
		{
			get => _mainForm.Scene;
			set => _mainForm.Scene = value;
		}

		public ForwardRenderer Renderer { get => _mainForm.Renderer; }
		private SpriteBatch _spriteBatch;

		public StudioGame()
		{
			Instance = this;

			// Restore state
			_state = State.Load();

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

			if (_state != null)
			{
				_graphics.PreferredBackBufferWidth = _state.Size.X;
				_graphics.PreferredBackBufferHeight = _state.Size.Y;
			}
			else
			{
				_graphics.PreferredBackBufferWidth = 1280;
				_graphics.PreferredBackBufferHeight = 800;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// UI
			MyraEnvironment.Game = this;
			Nrs.Game = this;
			Nrs.ExternalEffectsSource = new FolderWatcher(@"D:\Projects\Nursia\Nursia\EffectsSource")
			{
				BinaryFolder = @"D:\Projects\Nursia\Nursia\EffectsSource\FNA\bin"
			};

			_mainForm = new MainForm();

			if (_state != null)
			{
				_mainForm._topSplitPane.SetSplitterPosition(0, _state != null ? _state.TopSplitterPosition : 0.75f);
				_mainForm._leftSplitPane.SetSplitterPosition(0, _state != null ? _state.LeftSplitterPosition : 0.5f);
				_mainForm.LoadSolution(_state.EditedFile);
			}

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			var baseFolder = @"D:\Temp\Nursia\scenes\scene1";
			var assetManager = AssetManager.CreateFileAssetManager(baseFolder);
			var scene = Scene.Load(Path.Combine(baseFolder, @"scene.json"), assetManager);

			_mainForm.Scene = scene;

			_mainForm.BasePath = baseFolder;
			_mainForm.AssetManager = assetManager;

			// Skybox
			var texture = assetManager.LoadTextureCube(GraphicsDevice, "../../skybox/SkyBox.dds");
			scene.Skybox = new Skybox(100)
			{
				Texture = texture
			};

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			var assetFolder = Path.Combine(Utils.ExecutingAssemblyDirectory, "Assets");
			ModelStorage.Load(Path.Combine(assetFolder, "models"));
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
			_spriteBatch.End();
		}

		protected override void EndRun()
		{
			base.EndRun();


			var state = new State
			{
				Size = new Point(GraphicsDevice.PresentationParameters.BackBufferWidth,
					GraphicsDevice.PresentationParameters.BackBufferHeight),
				TopSplitterPosition = _mainForm._topSplitPane.GetSplitterPosition(0),
				LeftSplitterPosition = _mainForm._leftSplitPane.GetSplitterPosition(0),
				EditedFile = _mainForm.FilePath
			};

			state.Save();
		}
	}
}