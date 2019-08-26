using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class Mesh: ItemWithId
	{
		private readonly List<MeshPart> _parts = new List<MeshPart>();

		public List<MeshPart> Parts
		{
			get
			{
				return _parts;
			}
		}

		public Matrix Transform
		{
			get
			{
				return Matrix.Identity;
			}
		}

		public void Draw(RenderContext context)
		{
			using (var scope = new TransformScope(context, Transform))
			{
				foreach (var part in _parts)
				{
					part.Draw(context);
				}
			}
		}
	}
}