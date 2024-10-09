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
using System.Collections.Generic;
using System.IO;

namespace ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private NursiaModel _model;
		private CameraInputController _controller;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private MainPanel _mainPanel;
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private readonly Scene _scene = new Scene();
		private Desktop _desktop;
		private bool _isAnimating;
		private DateTime? _animationMoment;
		private static readonly List<DirectLight> _defaultLights = new List<DirectLight>();

		static ViewerGame()
		{
			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(-0.5265408f, -0.5735765f, -0.6275069f),
				Color = new Color(1, 0.9607844f, 0.8078432f)
			});

			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(0.7198464f, 0.3420201f, 0.6040227f),
				Color = new Color(0.9647059f, 0.7607844f, 0.4078432f)
			});

			_defaultLights.Add(new DirectLight
			{
				Direction = new Vector3(0.4545195f, -0.7660444f, 0.4545195f),
				Color = new Color(0.3231373f, 0.3607844f, 0.3937255f)
			});
		}

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
			_animationMoment = null;
		}

		private void LoadModel(string file)
		{
			try
			{
				if (!string.IsNullOrEmpty(file))
				{
					var manager = AssetManager.CreateFileAssetManager(Path.GetDirectoryName(file));
					_model = manager.LoadGltf(Path.GetFileName(file));

					_mainPanel._comboAnimations.Items.Clear();
					_mainPanel._comboAnimations.Items.Add(new ListItem(null));
					foreach (var pair in _model.Animations)
					{
						_mainPanel._comboAnimations.Items.Add(
							new ListItem(pair.Key)
							{
								Tag = pair.Value
							});
					}

					var current = _scene.QueryByType<NursiaModelNode>();
					foreach (var c in current)
					{
						_scene.Children.Remove(c);
					}

					_scene.Children.Add(new NursiaModelNode
					{
						Model = _model
					});
				}

				_mainPanel._textPath.Text = file;

				// Reset camera
				if (_model != null)
				{
					var min = _model.BoundingBox.Min;
					var max = _model.BoundingBox.Max;
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
			LoadModel(string.Empty);

			foreach (var l in _defaultLights)
			{
				_scene.Children.Add(l);
			}

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
			_animationMoment = null;
		}

		private void _sliderTime_ValueChanged(object sender, ValueChangedEventArgs<float> e)
		{
			if (_model == null || _model.CurrentAnimation == null)
			{
				return;
			}

			var k = (e.NewValue - _mainPanel._sliderTime.Minimum) / (_mainPanel._sliderTime.Maximum - _mainPanel._sliderTime.Minimum);

			var passed = _model.CurrentAnimation.Time * k;

			_mainPanel._labelTime.Text = passed.ToString();
			_model.UpdateCurrentAnimation(passed);
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

		private void _comboAnimations_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (_mainPanel._comboAnimations.SelectedItem == null)
			{
				_model.CurrentAnimation = null;
			}
			else
			{
				_model.CurrentAnimation = (AnimationClip)_mainPanel._comboAnimations.SelectedItem.Tag;
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

		private void DrawModel()
		{
			if (_model == null)
			{
				return;
			}

			if (_isAnimating && _model.CurrentAnimation != null)
			{
				if (_animationMoment != null)
				{
					var slider = _mainPanel._sliderTime;
					var sliderPart = (slider.Value - slider.Minimum) / (slider.Maximum - slider.Minimum);

					var passed = (float)(DateTime.Now - _animationMoment.Value).TotalSeconds;
					sliderPart += (passed / _model.CurrentAnimation.Time);

					if (sliderPart >= 1.0f)
					{
						sliderPart = 0.0f;
					}

					slider.Value = slider.Minimum + (slider.Maximum - slider.Minimum) * sliderPart;
				}

				_animationMoment = DateTime.Now;
			}

			_renderer.Render(_scene.Camera);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			DrawModel();

			_mainPanel._labelCamera.Text = "Camera: " + _scene.Camera.ToString();
			_mainPanel._labelFps.Text = "FPS: " + _fpsCounter.FramesPerSecond;
			//			_mainPanel._labelMeshes.Text = "Meshes: " + _renderer._statistics.MeshesDrawn;

			_desktop.Render();

			_fpsCounter.Draw(gameTime);
		}
	}
}