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

					_mainPanel._comboAnimations.Widgets.Clear();
					_mainPanel._comboAnimations.Widgets.Add(new Label());
					foreach (var pair in model.Animations)
					{
						_mainPanel._comboAnimations.Widgets.Add(
							new Label
							{
								Text = pair.Key,
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
			_mainPanel._comboAnimations.Widgets.Clear();
			_mainPanel._comboAnimations.SelectedIndexChanged += _comboAnimations_SelectedIndexChanged;

			_mainPanel._comboPlaybackMode.SelectedIndex = 0;
			_mainPanel._comboPlaybackMode.SelectedIndexChanged += (s, a) =>
			{
				_player.PlaybackMode = (PlaybackMode)_mainPanel._comboPlaybackMode.SelectedIndex.Value;
			};

			_mainPanel._sliderSpeed.ValueChanged += (s, a) =>
			{
				_mainPanel._labelSpeed.Text = _mainPanel._sliderSpeed.Value.ToString("0.00");
				_player.Speed = _mainPanel._sliderSpeed.Value;
			};
			

			_mainPanel._buttonChange.Click += _buttonChange_Click;

			_mainPanel._sliderTime.ValueChangedByUser += _sliderTime_ValueChanged;
			_mainPanel._sliderTime.ValueChanged += (s, a) =>
			{
				_mainPanel._labelTime.Text = _mainPanel._sliderTime.Value.ToString("0.00");
			};

			_mainPanel._buttonPlayStop.Click += _buttonPlayStop_Click;
			_mainPanel._buttonBoundingBoxes.PressedChanged += _buttonBoundingBoxes_PressedChanged;
			_mainPanel._buttonShadowMap.PressedChanged += _buttonShadowMap_PressedChanged;

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

			_player.TimeChanged += (s, a) =>
			{
				if (_player.AnimationClip == null)
				{
					return;
				}

				var k = (float)(_player.Time / _player.AnimationClip.Duration);

				var slider = _mainPanel._sliderTime;
				slider.Value = slider.Minimum + k * (slider.Maximum - slider.Minimum);
			};

			LoadModel(string.Empty);

			_controller = new CameraInputController(_scene.Camera);
		}

		private void _buttonShadowMap_PressedChanged(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void _buttonBoundingBoxes_PressedChanged(object sender, EventArgs e)
		{
			DebugSettings.DrawBoundingBoxes = _mainPanel._buttonBoundingBoxes.IsPressed;
		}

		private void _buttonPlayStop_Click(object sender, EventArgs e)
		{
			_isAnimating = !_isAnimating;

			var label = (Label)_mainPanel._buttonPlayStop.Content;
			label.Text = _isAnimating ? "Stop" : "Play";
		}

		private void _sliderTime_ValueChanged(object sender, ValueChangedEventArgs<float> e)
		{
			if (!_player.IsPlaying)
			{
				return;
			}

			var k = (e.NewValue - _mainPanel._sliderTime.Minimum) / (_mainPanel._sliderTime.Maximum - _mainPanel._sliderTime.Minimum);
			var passed = _player.AnimationClip.Duration * k;
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
			if (_mainPanel._comboAnimations.SelectedItem == null || string.IsNullOrEmpty(((Label)_mainPanel._comboAnimations.SelectedItem).Text))
			{
				_player.StopClip();
			}
			else
			{
				var clipName = ((Label)_mainPanel._comboAnimations.SelectedItem).Text;
				if (_mainPanel._checkCrossfade.IsChecked)
				{
					_player.CrossFade(clipName, TimeSpan.FromSeconds(0.5f));
				} else
				{
					_player.StartClip(clipName);
				}
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

			if (_isAnimating)
			{
				_player.Update(gameTime.ElapsedGameTime);
			}

			_renderer.AddNode(_scene);
			_renderer.Render(_scene.Camera);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawModel(gameTime);

			var stats = _renderer.Statistics;

			_mainPanel._labelEffectsSwitches.Text = stats.EffectsSwitches.ToString();
			_mainPanel._labelDrawCalls.Text = stats.DrawCalls.ToString();
			_mainPanel._labelVerticesDrawn.Text = stats.VerticesDrawn.ToString();
			_mainPanel._labelPrimitivesDrawn.Text = stats.PrimitivesDrawn.ToString();
			_mainPanel._labelMeshesDrawn.Text = stats.MeshesDrawn.ToString();

			_desktop.Render();

			_fpsCounter.Draw(gameTime);
		}
	}
}