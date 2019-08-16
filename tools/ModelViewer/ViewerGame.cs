using Microsoft.Xna.Framework;
using Nursia;
using Nursia.Graphics3D;
using System.IO;

namespace ModelViewer
{
	public class ViewerGame : Game
	{
		private readonly GraphicsDeviceManager _graphics;
		private Sprite3D _model;
		private readonly Renderer _renderer = new Renderer();

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

			Nrs.Game = this;

			/*			var data = File.ReadAllText(@"D:\Projects\Nursia\samples\box.n3t");
						_model = Sprite3D.LoadFromJson(data, null);*/

			_model = new Sprite3D();
			_model.Meshes.Add(PrimitivesFactory.CreateCube(10));

			foreach(var mesh in _model.Meshes)
			{
				mesh.Material = new BaseMaterial
				{
					HasLight = true,
					DiffuseColor = Color.White
				};
			}

			_renderer.Lights.Add(new Nursia.Graphics3D.Lights.DirectionalLight
			{
				Color = Color.Yellow,
				Direction = new Vector3(0, 1,-0.5f)
			});

			var camera = new PerspectiveCamera();
			_renderer.Camera = camera;
		}

		int angle = 0;


		protected override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			GraphicsDevice.Clear(Color.Black);

			_model.Meshes[0].Transform = Matrix.CreateRotationY(MathHelper.ToRadians(angle));

			_renderer.Render(_model);

			++angle;
		}
	}
}