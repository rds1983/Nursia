using System;
using System.Reflection;
using System.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DirectionalLight = Nursia.Graphics3D.Environment.DirectionalLight;
using System.Collections.Generic;
using Nursia.Graphics3D.Modeling;

namespace Nursia.Graphics3D.Rendering
{
	public class ModelBatch
	{
		private BasicEffect _basicEffect;

		private Camera _camera;
		private DirectionalLight[] _lights;
		private readonly List<ModelInstance> _items = new List<ModelInstance>();

		public Camera Camera
		{
			get
			{
				return _camera;
			}
		}

		public List<ModelInstance> Items
		{
			get
			{
				return _items;
			}
		}

		public void Begin(Camera camera, DirectionalLight[] lights = null)
		{
			if (camera == null)
			{
				throw new ArgumentNullException("camera");
			}

			_camera = camera;
			_lights = lights;
		}

		public void Add(ModelInstance mesh)
		{
			_items.Add(mesh);
		}

		public void End()
		{
			var device = Nrs.GraphicsDevice;

			// Set the View matrix which defines the camera and what it's looking at
			_camera.Viewport = new Vector2(device.Viewport.Width, device.Viewport.Height);

			var viewProjection = _camera.View * _camera.Projection;

			// Apply the effect and render items
			foreach (var item in _items)
			{
				var material = item.Material;
				if (material == null)
				{
					continue;
				}

				var effect = Assets.GetDefaultEffect(item.Material.HasLight, item.Material.Texture != null);

				var mesh = item.Model;
				device.SetVertexBuffer(mesh.VertexBuffer);
				device.Indices = mesh.IndexBuffer;

				var worldViewProj = item.Transform * viewProjection;
				var worldInverseTranspose = Matrix.Transpose(Matrix.Invert(item.Transform));

				effect.Parameters["_eyePosition"].SetValue(_camera.Position);
				effect.Parameters["_world"].SetValue(item.Transform);
				effect.Parameters["_worldViewProj"].SetValue(worldViewProj);
				effect.Parameters["_worldInverseTranspose"].SetValue(worldInverseTranspose);
				effect.Parameters["_diffuseColor"].SetValue(material.DiffuseColor.ToVector4());

				if (item.Material.Texture != null)
				{
					effect.Parameters["_texture"].SetValue(material.Texture);
				}

				if (material.HasLight)
				{
					if (_lights != null)
					{
						device.BlendState = BlendState.AlphaBlend;
						for (var i = 0; i < _lights.Length; ++i)
						{
							if (i == 1)
							{
								device.BlendState = BlendState.Additive;
							}

							var dl = _lights[i];

							effect.Parameters["_lightDir"].SetValue(dl.NormalizedDirection);
							effect.Parameters["_lightColor"].SetValue(dl.Color.ToVector3());


							foreach (var pass in effect.CurrentTechnique.Passes)
							{
								pass.Apply();
								device.DrawIndexedPrimitives(mesh.PrimitiveType, 0, 0, mesh.PrimitiveCount);
							}
						}
					}
				}
				else
				{
					foreach (var pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();

						device.DrawIndexedPrimitives(mesh.PrimitiveType, 0, 0, mesh.PrimitiveCount);
					}
				}
			}

			_items.Clear();
		}


		private static void SetMonoGameDirectionalLight(Microsoft.Xna.Framework.Graphics.DirectionalLight mgLight, DirectionalLight light)
		{
			mgLight.DiffuseColor = light.Color.ToVector3();
			mgLight.SpecularColor = mgLight.DiffuseColor;
			mgLight.Direction = light.Direction;
			mgLight.Enabled = true;
		}

		public void End2()
		{
			var device = Nrs.GraphicsDevice;

			if (_basicEffect == null)
			{
				_basicEffect = new BasicEffect(device);
			}

			// Set the View matrix which defines the camera and what it's looking at
			_camera.Viewport = new Vector2(device.Viewport.Width, device.Viewport.Height);

			_basicEffect.View = _camera.View;
			_basicEffect.Projection = _camera.Projection;

			// Apply the effect and render items
			var lights = new List<DirectionalLight>();
			foreach (var item in _items)
			{
				if (item.Material == null)
				{
					continue;
				}

				if (item.Material.HasLight)
				{
					_basicEffect.LightingEnabled = true;

					if (lights.Count > 0)
					{
						SetMonoGameDirectionalLight(_basicEffect.DirectionalLight0, lights[0]);
					} else
					{
						_basicEffect.DirectionalLight0.Enabled = false;
					}

					if (lights.Count > 1)
					{
						SetMonoGameDirectionalLight(_basicEffect.DirectionalLight1, lights[1]);
					}
					else
					{
						_basicEffect.DirectionalLight1.Enabled = false;
					}

					if (lights.Count > 2)
					{
						SetMonoGameDirectionalLight(_basicEffect.DirectionalLight2, lights[2]);
					}
					else
					{
						_basicEffect.DirectionalLight2.Enabled = false;
					}
				}
				else
				{
					_basicEffect.LightingEnabled = false;
				}

				_basicEffect.DiffuseColor = item.Material.DiffuseColor.ToVector3();
				_basicEffect.World = item.Transform;
				var mesh = item.Model;
				device.SetVertexBuffer(mesh.VertexBuffer);
				device.Indices = mesh.IndexBuffer;
				foreach (var pass in _basicEffect.CurrentTechnique.Passes)
				{
					pass.Apply();

					device.DrawIndexedPrimitives(mesh.PrimitiveType, 0, 0, mesh.VertexBuffer.VertexCount, 0, 
						mesh.PrimitiveCount);
				}
			}

			_items.Clear();
		}
	}
}
