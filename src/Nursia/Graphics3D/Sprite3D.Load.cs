using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiniJSON;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nursia.Graphics3D.Modeling
{
	partial class Sprite3D
	{
		private readonly static Dictionary<Type, Type> _loaders = new Dictionary<Type, Type>();

		static Sprite3D()
		{
			_loaders[typeof(VertexPositionNormalTexture)] = typeof(VertexPositionNormalTextureLoader);
		}

		public static Sprite3D LoadFromJson(string json, Func<string, Texture2D> textureGetter)
		{
			var graphicsAssembly = typeof(VertexPosition).Assembly;
			var vertexName = typeof(VertexPosition).Namespace;

			var root = (Dictionary<string, object>)Json.Deserialize(json);

			var meshes = (List<object>)root["meshes"];
			foreach(Dictionary<string, object> mesh in meshes)
			{
				// Determine vertex type
				var declarationTypeName = (string)mesh["declaration"];
				var declarationType = graphicsAssembly.GetType(vertexName + "." + declarationTypeName);

				if (declarationType == null)
				{
					throw new Exception(string.Format("Could not recognize vertex type '{0}'", declarationTypeName));
				}

				Type loaderType;
				if (!_loaders.TryGetValue(declarationType, out loaderType))
				{
					throw new Exception(string.Format("Loader for vertex type '{0}' isn't registered", declarationTypeName));
				}

				var loader = (IVertexLoader)Activator.CreateInstance(loaderType);
				var vertices = (List<object>)mesh["vertices"];
				var data = loader.ReadData(vertices);

				var k = 5;
			}


			return null;
		}
	}
}
