using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Nursia.Rendering;
using Nursia;
using NursiaEditor.UI;

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

		public ForwardRenderer Renderer => _mainForm.Renderer;

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

			/*			Nrs.ExternalEffectsSource = new FolderWatcher(@"D:\Projects\Nursia\Nursia\EffectsSource")
						{
							BinaryFolder = @"D:\Projects\Nursia\Nursia\EffectsSource\FNA\bin"
						};*/

			_mainForm = new MainForm();

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainForm);

			if (_state != null)
			{
				_mainForm._topSplitPane.SetSplitterPosition(0, _state != null ? _state.TopSplitterPosition : 0.75f);
				_mainForm._leftSplitPane.SetSplitterPosition(0, _state != null ? _state.LeftSplitterPosition : 0.5f);

				if (!string.IsNullOrEmpty(_state.EditedFile))
				{
					_mainForm.Load(_state.EditedFile);
				}
			}
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

			var cameraString = (Scene != null && Scene.Camera != null) ? Scene.Camera.ToString() : string.Empty;

			_mainForm._labelCamera.Text = "Camera: " + cameraString;
			_mainForm._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;
			_mainForm._labelMeshes.Text = "Meshes: " + Renderer.Statistics.MeshesDrawn;

			_desktop.Render();

			_fpsCounter.Draw(gameTime);
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