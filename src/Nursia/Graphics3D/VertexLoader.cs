using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public interface IVertexLoader
	{
		VertexDeclaration VertexDeclaration { get; }

		object ReadData(List<object> data);
	}

	public abstract class VertexLoader<T>: IVertexLoader where T: IVertexType
	{
		private VertexDeclaration _vertexDeclaration;

		public VertexDeclaration VertexDeclaration
		{
			get
			{
				if (_vertexDeclaration != null)
				{
					return _vertexDeclaration;
				}

				IVertexType type = Activator.CreateInstance(typeof(T)) as IVertexType;
				if (type == null)
				{
					throw new ArgumentException("vertexData does not inherit IVertexType");
				}

				_vertexDeclaration = type.VertexDeclaration;
				if (_vertexDeclaration == null)
				{
					throw new ArgumentException("vertexType's VertexDeclaration cannot be null");
				}

				return _vertexDeclaration;
			}
		}

		public abstract int FloatsPerElement { get; }

		public object ReadData(List<object> data)
		{
			if (data.Count % FloatsPerElement != 0)
			{
				throw new Exception(
					string.Format("Inconsistent vertex data size: {0} % {1} != 0", data.Count, FloatsPerElement));
			}

			var size = data.Count / FloatsPerElement;

			var result = new T[size];
			var floatIndex = 0;
			var elementIndex = 0;
			while(floatIndex < data.Count)
			{
				result[elementIndex] = ReadElement(data, ref floatIndex);
				++elementIndex;
			}

			return result;
		}

		protected float ReadFloat(List<object> data, ref int index)
		{
			var oldIndex = index;
			++index;

			return data[oldIndex].ToFloat();
		}

		protected abstract T ReadElement(List<object> data, ref int index);
	}

	internal class VertexPositionNormalTextureLoader : VertexLoader<VertexPositionNormalTexture>
	{
		public override int FloatsPerElement => 8;

		protected override VertexPositionNormalTexture ReadElement(List<object> data, ref int floatIndex)
		{
			return new VertexPositionNormalTexture(
				new Vector3
				{
					X = ReadFloat(data, ref floatIndex),
					Y = ReadFloat(data, ref floatIndex),
					Z = ReadFloat(data, ref floatIndex)
				},
				new Vector3
				{
					X = ReadFloat(data, ref floatIndex),
					Y = ReadFloat(data, ref floatIndex),
					Z = ReadFloat(data, ref floatIndex)
				},
				new Vector2
				{
					X = ReadFloat(data, ref floatIndex),
					Y = ReadFloat(data, ref floatIndex)
				}
			);
		}
	}
}
