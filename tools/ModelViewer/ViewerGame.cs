using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using Nursia.Graphics3D;
using Nursia.Graphics3D.Scene;
using Nursia.Graphics3D.Utils;
using System.IO;

namespace ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Sprite3D _model;
		private readonly RenderContext _renderState = new RenderContext();
		private CameraInputController _controller;
		private SpriteBatch _spriteBatch;

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

			var data = File.ReadAllText(@"D:\Projects\Nursia\samples\box.n3t");
			_model = Sprite3D.LoadFromJson(data, 
				n => File.OpenRead(Path.Combine(@"D:\Projects\Nursia\samples", n)));

/*			_model = new Sprite3D();
			var newMesh = PrimitivesFactory.CreateCube(1);
			_model.Meshes.Add(newMesh);*/

			_renderState.Lights.Add(new Nursia.Graphics3D.Lights.DirectionalLight
			{
				Color = Color.White,
				Direction = new Vector3(1.0f, 1.0f, -1.0f)
			});

/*			_renderer.Lights.Add(new Nursia.Graphics3D.Lights.DirectionalLight
			{
				Color = Color.White,
				Direction = new Vector3(-1.0f, -1.0f, -1.0f)
			});*/

			var camera = new PerspectiveCamera
			{
				Position = new Vector3(0, 0, 50.0f),
				PitchAngle = 180
			};

			_controller = new CameraInputController(camera);
			_renderState.Camera = camera;
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
			_controller.SetMousePosition(mouseState.Position);

			_controller.Update();
		}

		int angle = 0;

		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_model.Meshes[0].Transform = Matrix.CreateScale(20) * Matrix.CreateRotationY(MathHelper.ToRadians(angle));

			GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

			_model.Draw(_renderState);

			var camera = _renderState.Camera;

			_spriteBatch.Begin();

			_spriteBatch.DrawString(Assets.DebugFont, "Position: " + camera.Position, Vector2.Zero, Color.White);
			_spriteBatch.DrawString(Assets.DebugFont, "Yaw: " + camera.YawAngle, new Vector2(0, 32), Color.White);
			_spriteBatch.DrawString(Assets.DebugFont, "Pitch: " + camera.PitchAngle, new Vector2(0, 64), Color.White);

			_spriteBatch.End();

			++angle;
		}
	}
}