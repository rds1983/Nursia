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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Rendering;
using Nursia.Utilities;
using System;

namespace Nursia.Primitives
{
	public class Plane : PrimitiveMesh
	{
		private Vector2 _size;
		private int _tessellation = 1;
		private Vector2 _uvFactor;

		public Vector2 Size
		{
			get => _size;

			set
			{
				if (value.EpsilonEquals(_size))
				{
					return;
				}

				_size = value;
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

		public Vector2 UvFactor
		{
			get => _uvFactor;

			set
			{
				if (value.EpsilonEquals(_uvFactor))
				{
					return;
				}

				_uvFactor = value;
				InvalidateMesh();
			}
		}

		protected override Mesh CreateMesh()
		{
			if (_tessellation < 1)
			{
				throw new ArgumentOutOfRangeException("tessellation", "tessellation must be > 0");
			}

			var builder = new Builder();
			var lineWidth = _tessellation + 1;

			var deltaX = _size.X / _tessellation;
			var deltaY = _size.Y / _tessellation;

			_size.X /= 2.0f;
			_size.Y /= 2.0f;

			var normal = Vector3.UnitZ;

			// Create vertices
			for (int y = 0; y < (_tessellation + 1); y++)
			{
				for (int x = 0; x < (_tessellation + 1); x++)
				{
					var position = new Vector3(-_size.X + deltaX * x, _size.Y - deltaY * y, 0);
					var texCoord = new Vector2(1.0f * x / _tessellation * _uvFactor.X, 1.0f * y / _tessellation * _uvFactor.Y);
					builder.Vertices.Add(new VertexPositionNormalTexture(position, normal, texCoord));
				}
			}

			// Create indices
			for (int y = 0; y < _tessellation; y++)
			{
				for (int x = 0; x < _tessellation; x++)
				{
					// Six indices (two triangles) per face.
					short vbase = (short)(lineWidth * y + x);

					builder.Indices.Add((short)(vbase + 1));
					builder.Indices.Add((short)(vbase + 1 + lineWidth));
					builder.Indices.Add((short)(vbase + lineWidth));

					builder.Indices.Add((short)(vbase + 1));
					builder.Indices.Add((short)(vbase + lineWidth));
					builder.Indices.Add((short)(vbase));
				}
			}

			// Create the primitive object.
			return builder.Create(IsLeftHanded);
		}
	}
}