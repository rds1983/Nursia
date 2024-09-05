// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTk http://directxtk.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the 
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and 
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to 
// the software.
// A "contributor" is any person that distributes its contribution under this 
// license.
// "Licensed patents" are a contributor's patent claims that read directly on 
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the 
// license conditions and limitations in section 3, each contributor grants 
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and 
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a 
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution 
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any 
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that 
// you claim are infringed by the software, your patent license from such 
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all 
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you 
// may do so only under this license by including a complete copy of this 
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that 
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license 
// cannot change. To the extent permitted under your local laws, the 
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework;
using Nursia.Utilities;
using Nursia.Rendering;

namespace Nursia.Primitives
{
	public class Cylinder : PrimitiveMesh
	{
		private float _height = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 32;

		public float Height
		{
			get => _height;

			set
			{
				if (value.EpsilonEquals(_height))
				{
					return;
				}

				_height = value;
				InvalidateMesh();
			}
		}

		public float Radius
		{
			get => _radius;

			set
			{
				if (value.EpsilonEquals(_radius))
				{
					return;
				}

				_radius = value;
				InvalidateMesh();
			}
		}

		public int Tessellation
		{
			get => _tessellation;

			set
			{
				if (value == _tessellation)
				{
					return;
				}

				_tessellation = value;
				InvalidateMesh();
			}
		}

		// Helper computes a point on a unit circle, aligned to the x/z plane and centered on the origin.
		private static Vector3 GetCircleVector(int i, int tessellation)
		{
			var angle = (float)(i * 2.0 * Math.PI / tessellation);
			var dx = MathF.Sin(angle);
			var dz = MathF.Cos(angle);

			return new Vector3(dx, 0, dz);
		}

		// Helper creates a triangle fan to close the end of a cylinder.
		void CreateCylinderCap(Builder builder, float height, bool isTop)
		{
			// Create cap indices.
			for (int i = 0; i < _tessellation - 2; i++)
			{
				int i1 = (i + 1) % _tessellation;
				int i2 = (i + 2) % _tessellation;

				if (isTop)
				{
					Utils.Swap(ref i1, ref i2);
				}

				int vbase = builder.Vertices.Count;
				builder.Indices.Add(vbase);
				builder.Indices.Add(vbase + i1);
				builder.Indices.Add(vbase + i2);
			}

			// Which end of the cylinder is this?
			var normal = Vector3.UnitY;
			var textureScale = new Vector2(-0.5f);

			if (!isTop)
			{
				normal = -normal;
				textureScale.X = -textureScale.X;
			}

			// Create cap vertices.
			for (int i = 0; i < _tessellation; i++)
			{
				var circleVector = GetCircleVector(i, _tessellation);
				var position = (circleVector * _radius) + (normal * height);
				var textureCoordinate = new Vector2(UScale * (circleVector.X * textureScale.X + 0.5f), VScale * (circleVector.Z * textureScale.Y + 0.5f));

				builder.Vertices.Add(new VertexPositionNormalTexture(position, normal, textureCoordinate));
			}
		}

		protected override Mesh CreateMesh()
		{
			if (_tessellation < 3)
				throw new ArgumentOutOfRangeException("tessellation", "tessellation parameter out of range");

			var height = Height / 2;
			var stride = _tessellation + 1;
			var topOffset = Vector3.UnitY * height;

			var builder = new Builder();

			// Create a ring of triangles around the outside of the cylinder.
			for (int i = 0; i <= _tessellation; i++)
			{
				var normal = GetCircleVector(i, _tessellation);

				var sideOffset = normal * _radius;

				var textureCoordinate = new Vector2((float)i / _tessellation, 0);

				builder.Vertices.Add(new VertexPositionNormalTexture(sideOffset + topOffset, normal, textureCoordinate * new Vector2(UScale, VScale)));
				builder.Vertices.Add(new VertexPositionNormalTexture(sideOffset - topOffset, normal, (textureCoordinate + Vector2.UnitY) * new Vector2(UScale, VScale)));

				builder.Indices.Add(i * 2);
				builder.Indices.Add((i * 2 + 2) % (stride * 2));
				builder.Indices.Add(i * 2 + 1);

				builder.Indices.Add(i * 2 + 1);
				builder.Indices.Add((i * 2 + 2) % (stride * 2));
				builder.Indices.Add((i * 2 + 3) % (stride * 2));
			}

			// Create flat triangle fan caps to seal the top and bottom.
			CreateCylinderCap(builder, height, true);
			CreateCylinderCap(builder, height, false);

			// Create the primitive object.
			return builder.Create(IsLeftHanded);
		}
	}
}
