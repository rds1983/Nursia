using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra;
using Myra.Graphics2D.UI;
using Nursia;
using Nursia.Rendering;
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
		public static MainForm MainForm => Instance._mainForm;

		public StudioGame()
		{
			Instance = this;

			// Restore state
			_state = State.Load();
			if (_state != null)
			{
				NursiaEditorOptions.ShowGrid = _state.ShowGrid;
				DebugSettings.DrawBoundingBoxes = _state.DrawBoundingBoxes;
				DebugSettings.DrawLightViewFrustrum = _state.DrawLightViewFrustum;
				NursiaEditorOptions.DrawShadowMap = _state.DrawShadowMap;
			}

			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800,
				PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
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

			/*			Nrs.ExternalEffects = new FolderWatcher(@"D:\Projects\Nursia\Nursia\Effects")
						{
							BinaryFolder = @"D:\Projects\Nursia\Nursia\Effects\FNA\bin"
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
					_mainForm.LoadSolution(_state.EditedFile);
				}
			}

			DebugSettings.DrawCamerasFrustums = true;
			DebugSettings.DrawLights = true;

			NodesRegistry.AddAssembly(typeof(SceneNode).Assembly);
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
				EditedFile = _mainForm.FilePath,
				ShowGrid = NursiaEditorOptions.ShowGrid,
				DrawBoundingBoxes = DebugSettings.DrawBoundingBoxes,
				DrawLightViewFrustum = DebugSettings.DrawLightViewFrustrum,
				DrawShadowMap = NursiaEditorOptions.DrawShadowMap,
			};

			state.Save();
		}
	}
}