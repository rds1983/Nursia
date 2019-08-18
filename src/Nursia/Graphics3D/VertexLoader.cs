﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System;
using System.Collections.Generic;

namespace Nursia.Graphics3D
{
	public interface IVertexLoader
	{
		VertexDeclaration VertexDeclaration { get; }

		VertexBuffer CreateVertexBuffer(List<object> data);
	}

	public abstract class VertexLoader<T>: IVertexLoader where T: struct, IVertexType
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

				_vertexDeclaration = new T().VertexDeclaration;

				return _vertexDeclaration;
			}
		}

		public abstract int FloatsPerElement { get; }

		public VertexBuffer CreateVertexBuffer(List<object> input)
		{
			if (input.Count % FloatsPerElement != 0)
			{
				throw new Exception(
					string.Format("Inconsistent vertex data size: {0} % {1} != 0", input.Count, FloatsPerElement));
			}

			var size = input.Count / FloatsPerElement;

			var data = new T[size];
			var floatIndex = 0;
			var elementIndex = 0;
			while(floatIndex < input.Count)
			{
				data[elementIndex] = ReadElement(input, ref floatIndex);
				++elementIndex;
			}

			var result = new VertexBuffer(Nrs.GraphicsDevice, VertexDeclaration, size, BufferUsage.None);
			result.SetData(data);

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