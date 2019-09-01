using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModelViewer.UI;
using Myra;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.Scene;
using Nursia.Graphics3D.Utils;
using System;
using System.IO;
using System.Reflection;
using DirectionalLight = Nursia.Graphics3D.Lights.DirectionalLight;
using RenderContext = Nursia.Graphics3D.RenderContext;

namespace ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Sprite3D _model;
		private readonly Camera _camera = new Camera();
		private CameraInputController _controller;
		private ForwardRenderer _renderer;
		private readonly RenderContext _context = new RenderContext();
		private Desktop _desktop = null;
		private MainPanel _mainPanel;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();

		public ViewerGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = true;
		}

		private void LoadModel(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				var folder = Path.GetDirectoryName(file);
				var data = File.ReadAllText(file);
				_model = Sprite3D.LoadFromJson(data,
					n =>
					{
						using (var stream = File.OpenRead(Path.Combine(folder, n)))
						{
							return Texture2D.FromStream(GraphicsDevice, stream);
						}
					});

				_mainPanel._comboAnimations.Items.Clear();
				_mainPanel._comboAnimations.Items.Add(new ListItem(null));
				foreach (var pair in _model.Animations)
				{
					_mainPanel._comboAnimations.Items.Add(new ListItem(pair.Key));
				}
			}

			// Reset camera
			_camera.SetLookAt(new Vector3(10, 10, 10), Vector3.Zero);
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			MyraEnvironment.Game = this;

			Nrs.Game = this;

			_mainPanel = new MainPanel();
			_mainPanel._comboAnimations.Items.Clear();
			_mainPanel._comboAnimations.SelectedIndexChanged += _comboAnimations_SelectedIndexChanged;

			_mainPanel._buttonChange.Click += OnChangeFolder;

			_mainPanel._listFiles.SelectedIndexChanged += _listFiles_SelectedIndexChanged;

			_desktop = new Desktop();
			_desktop.Widgets.Add(_mainPanel);

			_renderer = new ForwardRenderer();

			LoadModel(string.Empty);

			var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			folder = @"D:\Projects\Nursia\samples\models";
			SetFolder(folder);

			_controller = new CameraInputController(_camera);
		}

		private void _listFiles_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_mainPanel._listFiles.SelectedItem == null)
			{
				LoadModel(null);
			} else
			{
				LoadModel(_mainPanel._listFiles.SelectedItem.Id);
			}
		}

		private void SetFolder(string folder)
		{
			_mainPanel._listFiles.Items.Clear();
			var files = Directory.EnumerateFiles(folder, "*.g3dj");
			foreach (var f in files)
			{
				var fileInfo = new FileInfo(f);
				if (fileInfo.Attributes.HasFlag(FileAttributes.Hidden))
				{
					continue;
				}

				_mainPanel._listFiles.Items.Add(new ListItem(fileInfo.Name)
				{
					Id = fileInfo.FullName
				});
			}

			_mainPanel._textPath.Text = folder;
		}

		private void OnChangeFolder(object sender, EventArgs e)
		{
;			var dlg = new FileDialog(FileDialogMode.ChooseFolder);

			try
			{
				if (!string.IsNullOrEmpty(_mainPanel._textPath.Text))
				{
					dlg.Folder = _mainPanel._textPath.Text;
				} else
				{
					var folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
					dlg.Folder = folder;
				}
			}
			catch (Exception)
			{
			}

			dlg.Closed += (s, a) =>
			{
				if (!dlg.Result)
				{
					return;
				}

				SetFolder(dlg.FilePath);
			};

			dlg.ShowModal(_desktop);
		}

		private void _comboAnimations_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (_mainPanel._comboAnimations.SelectedItem == null)
			{
				_model.CurrentAnimation = null;
			}
			else
			{
				_model.CurrentAnimation = _mainPanel._comboAnimations.SelectedItem.Text;
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
			if (_model == null)
			{
				return;
			}

			_model.UpdateCurrentAnimation();

			_context.View = _camera.View;
			_context.Projection = Matrix.CreatePerspectiveFieldOfView(
					MathHelper.ToRadians(67.0f),
					_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
					0.1f,
					1000.0f);

			_renderer.Begin();
			_renderer.DrawModel(_model, _context);
			_renderer.End();
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawModel();

			_mainPanel._labelCamera.Text = "Camera: " + _camera.ToString();
			_mainPanel._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;

			_desktop.Bounds = new Rectangle(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth,
				  GraphicsDevice.PresentationParameters.BackBufferHeight);
			_desktop.Render();

			_fpsCounter.Draw(gameTime);
		}
	}
}