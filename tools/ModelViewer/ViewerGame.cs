using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.Scene;
using Nursia.Graphics3D.Utils;
using System.IO;

using DirectionalLight = Nursia.Graphics3D.Lights.DirectionalLight;

namespace ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Sprite3D _model;
		private Camera _camera;
		private CameraInputController _controller;
		private SpriteBatch _spriteBatch;
		private ForwardRenderer _renderer;
		private readonly RenderContext _context = new RenderContext();

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

		protected override void LoadContent()
		{
			base.LoadContent();

			_spriteBatch = new SpriteBatch(GraphicsDevice);

			Nrs.Game = this;

			_renderer = new ForwardRenderer();

			var data = File.ReadAllText(@"D:\Projects\Nursia\samples\models\skeleton\Model.g3dj");
			_model = Sprite3D.LoadFromJson(data,
				n =>
				{
					using (var stream = File.OpenRead(Path.Combine(@"D:\Projects\Nursia\samples\models\skeleton", n)))
					{
						return Texture2D.FromStream(GraphicsDevice, stream);
					}
				});

			_model.CurrentAnimation = "Skeleton01_anim_walk";

			/*			_model = new Sprite3D();
						var newMesh = PrimitivesFactory.CreateCube(1);
						_model.Meshes.Add(newMesh);*/

			_context.Lights.Add(new DirectionalLight
			{
				Color = Color.White,
				Direction = new Vector3(1.0f, 0.0f, -1.0f)
			});

			_context.Lights.Add(new DirectionalLight
			{
				Color = Color.Red,
				Direction = new Vector3(-1.0f, 0.0f, -1.0f)
			});

			_camera = new Camera(
				new Vector3(0, 0, 50),
				Vector3.Zero,
				Vector3.Up);

			_controller = new CameraInputController(_camera);
		}

		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			var keyboardState = Keyboard.GetState();
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
				Exit();

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

		int angle = 0;

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_model.UpdateCurrentAnimation();
			//			_model.Transform = Matrix.CreateRotationZ(MathHelper.ToRadians(angle));

			_context.View = _camera.View;
			_context.Projection = Matrix.CreatePerspectiveFieldOfView(
					MathHelper.ToRadians(67.0f),
					_graphics.PreferredBackBufferWidth / _graphics.PreferredBackBufferHeight,
					1.0f,
					1000.0f);

			_renderer.Begin();
			_renderer.DrawModel(_model, _context);
			_renderer.End();

			_spriteBatch.Begin();

			_spriteBatch.DrawString(Assets.DebugFont, "Position: " + _camera.Position, Vector2.Zero, Color.White);
			_spriteBatch.DrawString(Assets.DebugFont, "Yaw: " + _camera.YawAngle, new Vector2(0, 32), Color.White);
			_spriteBatch.DrawString(Assets.DebugFont, "Pitch: " + _camera.PitchAngle, new Vector2(0, 64), Color.White);
			_spriteBatch.End();

			++angle;
			if (angle >= 360)
			{
				angle = 0;
			}
		}
	}
}