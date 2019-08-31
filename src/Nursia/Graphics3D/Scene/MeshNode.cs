using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class MeshNode : Node
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
			foreach (var part in _parts)
			{
				// If part has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				using (var scope = new TransformScope(context,
					part.BonesPerMesh != BonesPerMesh.None ? Transform : AbsoluteTransform))
				{
					part.Draw(context);
				}
			}
		}
	}
}