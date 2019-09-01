using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D
{
	internal struct TransformScope: IDisposable
	{
		private readonly Context3d _context;
		private readonly Matrix _oldTransform;

		public TransformScope(Context3d context, Matrix transform)
		{
			_context = context;
			_oldTransform = context.World;
			_context.World = transform * _context.World;
		}

		public void Dispose()
		{
			_context.World = _oldTransform;
		}
	}
}
