using Microsoft.Xna.Framework;
using System;

namespace Nursia.Graphics3D
{
	internal struct TransformScope: IDisposable
	{
		private readonly RenderContext _context;
		private readonly Matrix _oldTransform;

		public TransformScope(RenderContext context, Matrix transform)
		{
			_context = context;
			_oldTransform = context.Transform;
			_context.Transform = transform * _context.Transform;
		}

		public void Dispose()
		{
			_context.Transform = _oldTransform;
		}
	}
}
