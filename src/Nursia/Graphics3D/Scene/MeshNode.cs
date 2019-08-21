using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class MeshNode : Node
	{
		private readonly List<MeshNodePart> _parts = new List<MeshNodePart>();

		public List<MeshNodePart> Parts
		{
			get
			{
				return _parts;
			}
		}

		public override void Draw(RenderContext context)
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