using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using Nursia.Animation;
using Nursia.Modelling;
using Nursia.Rendering;
using System;
using System.IO;
using System.Reflection;

namespace SimpleScene
{
	public class ViewerGame : Game
	{
		private const float MouseSensitivity = 0.2f;
		private const float MovementSpeed = 0.05f;

		private readonly GraphicsDeviceManager _graphics;
		private Scene _scene;
		private SceneNode _cameraMount;
		private Camera _mainCamera;
		private NursiaModelNode _model;
		private AnimationController _player;
		private readonly ForwardRenderer _renderer = new ForwardRenderer();
		private readonly FramesPerSecondCounter _fpsCounter = new FramesPerSecondCounter();
		private SpriteBatch _spriteBatch;
		private InputService _inputService;

		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public ViewerGame()
		{
			_graphics = new GraphicsDeviceManager(this)
			{
				PreferredBackBufferWidth = 1200,
				PreferredBackBufferHeight = 800
			};

			Window.AllowUserResizing = true;
			IsMouseVisible = false;

			if (Configuration.NoFixedStep)
			{
				IsFixedTimeStep = false;
				_graphics.SynchronizeWithVerticalRetrace = false;
			}
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			// Nursia
			Nrs.Game = this;

			var assetManager = AssetManager.CreateFileAssetManager(Path.Combine(ExecutingAssemblyDirectory, "Assets"));
			_scene = assetManager.LoadScene("Scenes/Main.scene");

			_model = _scene.QueryByType<NursiaModelNode>()[0];
			_player = new AnimationController(_model);
			_player.StartClip("idle");

			_cameraMount = _scene.QueryFirstById("_cameraMount");
			_mainCamera = _scene.QueryFirstByType<Camera>();

			_inputService = new InputService();
			_inputService.MouseMoved += _inputService_MouseMoved;

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			//			DebugSettings.DrawLightViewFrustrum = true;
		}

		private void _inputService_MouseMoved(object sender, InputEventArgs<Point> e)
		{
			var playerRotation = _model.Rotation;
			playerRotation.Y += -(int)((e.NewValue.X - e.OldValue.X) * MouseSensitivity);
			_model.Rotation = playerRotation;

			var cameraRotation = _cameraMount.Rotation;
			cameraRotation.X += (int)((e.NewValue.Y - e.OldValue.Y) * MouseSensitivity);
			_cameraMount.Rotation = cameraRotation;
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			_inputService.Update();

			var movement = 0;
			if (_inputService.IsKeyDown(Keys.W))
			{
				movement = -1;
			} else if (_inputService.IsKeyDown(Keys.S))
			{
				movement = 1;
			}

			if (_inputService.IsKeyDown(Keys.LeftShift) || _inputService.IsKeyDown(Keys.RightShift))
			{
				movement *= 2;
			}

			// Set animation
			switch(movement)
			{
				case 0:
					if (_player.AnimationClip.Name != "idle")
					{
						_player.StartClip("idle");
					}
					break;

				case 1:
				case -1:
					if (_player.AnimationClip.Name != "walking")
					{
						_player.StartClip("walking");
					}
					break;

				case 2:
				case -2:
					if (_player.AnimationClip.Name != "running")
					{
						_player.StartClip("running");
					}
					break;
			}

			// Perform the movement
			var velocity = _model.GlobalTransform.Forward * movement * MovementSpeed;
			_model.Translation += velocity;


			_fpsCounter.Update(gameTime);
			_player.Update(gameTime.ElapsedGameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_renderer.AddNode(_scene);
			_renderer.Render(_mainCamera);

			_fpsCounter.Draw(gameTime);

			_spriteBatch.Begin();

			/*			_spriteBatch.Draw(_light.ShadowMap, 
							new Rectangle(0, 0, 256, 256), 
							Color.White);*/

			_spriteBatch.End();
		}
	}
}
