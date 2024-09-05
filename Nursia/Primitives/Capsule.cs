// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
//
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;
using System;

namespace Nursia.Primitives
{
	public class Capsule : PrimitiveMesh
	{
		private float _length = 1.0f;
		private float _radius = 0.5f;
		private int _tessellation = 8;

		public float Length
		{
			get => _length;

			set
			{
				if (value.EpsilonEquals(_length))
				{
					return;
				}

				_length = value;
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

		protected override Mesh CreateMesh()
		{
			if (_tessellation < 3)
				throw new ArgumentOutOfRangeException("tessellation", "tessellation parameter out of range");

			int verticalSegments = 2 * _tessellation;
			int horizontalSegments = 4 * _tessellation;

			var builder = new Builder();

			// Create rings of vertices at progressively higher latitudes.
			for (int i = 0; i < verticalSegments; i++)
			{
				float v;
				float deltaY;
				float latitude;
				if (i < verticalSegments / 2)
				{
					deltaY = -_length / 2;
					v = 1.0f - (0.25f * i / (_tessellation - 1));
					latitude = (float)((i * Math.PI / (verticalSegments - 2)) - Math.PI / 2.0);
				}
				else
				{
					deltaY = _length / 2;
					v = 0.5f - (0.25f * (i - 1) / (_tessellation - 1));
					latitude = (float)(((i - 1) * Math.PI / (verticalSegments - 2)) - Math.PI / 2.0);
				}

				var dy = MathF.Sin(latitude);
				var dxz = MathF.Cos(latitude);

				// Create a single ring of vertices at this latitude.
				for (int j = 0; j <= horizontalSegments; j++)
				{
					float u = (float)j / horizontalSegments;

					var longitude = (float)(j * 2.0 * Math.PI / horizontalSegments);
					var dx = MathF.Sin(longitude);
					var dz = MathF.Cos(longitude);

					dx *= dxz;
					dz *= dxz;

					var normal = new Vector3(dx, dy, dz);
					var textureCoordinate = new Vector2(u * UScale, v * VScale);
					var position = _radius * normal + new Vector3(0, deltaY, 0);

					builder.Vertices.Add(new VertexPositionNormalTexture(position, normal, textureCoordinate));
				}
			}

			// Fill the index buffer with triangles joining each pair of latitude rings.
			int stride = horizontalSegments + 1;

			for (int i = 0; i < verticalSegments - 1; i++)
			{
				for (int j = 0; j <= horizontalSegments; j++)
				{
					int nextI = i + 1;
					int nextJ = (j + 1) % stride;

					builder.Indices.Add(i * stride + j);
					builder.Indices.Add(nextI * stride + j);
					builder.Indices.Add(i * stride + nextJ);

					builder.Indices.Add(i * stride + nextJ);
					builder.Indices.Add(nextI * stride + j);
					builder.Indices.Add(nextI * stride + nextJ);
				}
			}

			// Create the primitive object.
			return builder.Create(IsLeftHanded);
		}
	}
}