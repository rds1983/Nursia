using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class MeshNode: Node
	{
		private readonly List<MeshPart> _parts = new List<MeshPart>();

		public List<MeshPart> Parts
		{
			get
			{
				return _parts;
			}
		}

		public void Draw(RenderContext context)
		{
			using (var scope = new TransformScope(context, AbsoluteTransform))
			{
				foreach (var part in _parts)
				{
					part.Draw(context);
				}
			}
		}
	}
}