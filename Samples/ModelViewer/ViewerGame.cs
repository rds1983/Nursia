using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ModelViewer.UI;
using Myra;
using Myra.Events;
using Myra.Graphics2D.UI;
using Myra.Graphics2D.UI.File;
using Nursia;
using Nursia.Animation;
using Nursia.Lights;
using Nursia.Modelling;
using Nursia.Rendering;
using Nursia.Utilities;
using System;
using System.IO;

namespace ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private readonly NursiaModelNode _model = new NursiaModelNode();
		private AnimationController _player = null;
		private CameraInputController _controller;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private MainPanel _mainPanel;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly Scene _scene = new Scene();
		private Desktop _desktop;
		private bool _isAnimating;

		public ViewerGame()
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

		private void ResetAnimation()
		{
			_mainPanel._sliderTime.Value = _mainPanel._sliderTime.Minimum;
		}

		private void LoadModel(string file)
		{
			try
			{
				if (!string.IsNullOrEmpty(file))
				{
					var manager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(file));
					var model = manager.LoadGltf(Path.GetFileName(file));
					_model.Model = model;

					_mainPanel._comboAnimations.Items.Clear();
					_mainPanel._comboAnimations.Items.Add(new ListItem(null));
					foreach (var pair in model.Animations)
					{
						_mainPanel._comboAnimations.Items.Add(
							new ListItem(pair.Key)
							{
								Tag = pair.Value
							});
					}
				}

				_mainPanel._textPath.Text = file;

				// Reset camera
				if (_model.Model != null)
				{
					var bb = _model.CalculateBoundingBox();
					var min = bb.Min;
					var max = bb.Max;
					var center = (min + max) / 2;
					var cameraPosition = (max - center) * 3.0f + center;

					_scene.Camera.SetLookAt(cameraPosition, center);

					var size = Math.Max(max.X - min.X, max.Y - min.Y);
					size = Math.Max(size, max.Z - min.Z);

					_scene.Camera.NearPlaneDistance = size / 1000.0f;
					_scene.Camera.FarPlaneDistance = size * 10.0f;
				}
				else
				{
					_scene.Camera.SetLookAt(Vector3.One, Vector3.Zero);
				}

				ResetAnimation();
			}
			catch (Exception ex)
			{
				var messageBox = Dialog.CreateMessageBox("Error", ex.Message);
				messageBox.ShowModal(_desktop);
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// UI
			MyraEnvironment.Game = this;
			_mainPanel = new MainPanel();
			_mainPanel._comboAnimations.Items.Clear();
			_mainPanel._comboAnimations.SelectedIndexChanged += _comboAnimations_SelectedIndexChanged;

			_mainPanel._buttonChange.Click += _buttonChange_Click;

			_mainPanel._sliderTime.ValueChanged += _sliderTime_ValueChanged;

			_mainPanel._buttonPlayStop.Click += _buttonPlayStop_Click;
			_mainPanel._checkBoxShowBoundingBoxes.PressedChanged += _checkBoxShowBoundingBoxes_PressedChanged;
			_mainPanel._checkBoxLight1.PressedChanged += _checkBoxLight1_PressedChanged;
			_mainPanel._checkBoxLight2.PressedChanged += _checkBoxLight1_PressedChanged;
			_mainPanel._checkBoxLight3.PressedChanged += _checkBoxLight1_PressedChanged;

			_desktop = new Desktop
			{
				Root = _mainPanel
			};

			// Nursia
			Nrs.Game = this;

			_scene.Children.Add(new DirectLight
			{
				Rotation = new Vector3(325, 23, 0)
			});
			_scene.Children.Add(_model);
			_player = new AnimationController(_model);

			LoadModel(string.Empty);

			_controller = new CameraInputController(_scene.Camera);
		}

		private void UpdateLights()
		{
			var pl = _scene.QueryByType<PointLight>();
			foreach (var l in pl)
			{
				_scene.Children.Remove(l);
			}

			if (_mainPanel._checkBoxLight1.IsChecked)
			{
				_scene.Children.Add(new PointLight
				{
					Translation = new Vector3(0, 0, 2),
					Color = Color.Yellow
				});
			}

			if (_mainPanel._checkBoxLight2.IsChecked)
			{
				_scene.Children.Add(new PointLight
				{
					Translation = new Vector3(-2, 2, 0),
					Color = Color.Red
				});
			}

			if (_mainPanel._checkBoxLight3.IsChecked)
			{
				_scene.Children.Add(new PointLight
				{
					Translation = new Vector3(-2, 0, -2),
					Color = Color.Blue
				});
			}
		}

		private void _checkBoxLight1_PressedChanged(object sender, EventArgs e)
		{
			UpdateLights();
		}

		private void _checkBoxShowBoundingBoxes_PressedChanged(object sender, EventArgs e)
		{
			DebugSettings.DrawBoundingBoxes = _mainPanel._checkBoxShowBoundingBoxes.IsPressed;
		}

		private void _buttonPlayStop_Click(object sender, EventArgs e)
		{
			_isAnimating = !_isAnimating;
			_mainPanel._buttonPlayStop.Text = _isAnimating ? "Stop" : "Play";
		}

		private void _sliderTime_ValueChanged(object sender, ValueChangedEventArgs<float> e)
		{
			if (_model == null || _player.AnimationClip == null)
			{
				return;
			}

			var k = (e.NewValue - _mainPanel._sliderTime.Minimum) / (_mainPanel._sliderTime.Maximum - _mainPanel._sliderTime.Minimum);

			var passed = _player.AnimationClip.Duration * k;

			_mainPanel._labelTime.Text = passed.ToString();

			_player.Time = passed;
		}

		private void _buttonChange_Click(object sender, EventArgs e)
		{
			FileDialog dialog = new FileDialog(FileDialogMode.OpenFile)
			{
				Filter = "*.gltf|*.glb"
			};

			if (!string.IsNullOrEmpty(_mainPanel._textPath.Text))
			{
				dialog.Folder = Path.GetDirectoryName(_mainPanel._textPath.Text);
			}

			dialog.Closed += (s, a) =>
			{
				if (!dialog.Result)
				{
					// "Cancel" or Escape
					return;
				}

				// "Ok" or Enter
				LoadModel(dialog.FilePath);
			};

			dialog.ShowModal(_desktop);
		}

		private void _comboAnimations_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (_mainPanel._comboAnimations.SelectedItem == null || string.IsNullOrEmpty(_mainPanel._comboAnimations.SelectedItem.Text))
			{
				_player.StopClip();
			}
			else
			{
				_player.StartClip(_mainPanel._comboAnimations.SelectedItem.Text);
			}

			ResetAnimation();
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

			if (!_desktop.IsTouchOverGUI)
			{
				var mouseState = Mouse.GetState();
				_controller.SetTouchState(CameraInputController.TouchType.Rotate, mouseState.RightButton == ButtonState.Pressed);
				_controller.SetMousePosition(new Point(mouseState.X, mouseState.Y));
			}

			_controller.Update();
		}

		private void DrawModel(GameTime gameTime)
		{
			if (_model == null)
			{
				return;
			}

			if (_isAnimating && _player.IsPlaying)
			{
				var slider = _mainPanel._sliderTime;
				var sliderPart = (slider.Value - slider.Minimum) / (slider.Maximum - slider.Minimum);

				sliderPart += (float)(gameTime.ElapsedGameTime.TotalSeconds / _player.AnimationClip.Duration.TotalSeconds);

				if (sliderPart >= 1.0f)
				{
					sliderPart = 0.0f;
				}

				slider.Value = slider.Minimum + (slider.Maximum - slider.Minimum) * sliderPart;
			}

			_renderer.AddNode(_scene);
			_renderer.Render(_scene.Camera);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawModel(gameTime);

			_mainPanel._labelCamera.Text = "Camera: " + _scene.Camera.ToString();
			_mainPanel._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;
			//			_mainPanel._labelMeshes.Text = "Meshes: " + _renderer._statistics.MeshesDrawn;

			_desktop.Render();

			_fpsCounter.Draw(gameTime);
		}
	}
}