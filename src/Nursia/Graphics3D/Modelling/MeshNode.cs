using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class MeshNode : ModelNode
	{
		private readonly List<MeshPart> _parts = new List<MeshPart>();

		public List<MeshPart> Parts
		{
			get
			{
				return _parts;
			}
		}

		public BoundingSphere BoundingSphere { get; set; }

		public void Draw(Context3d context)
		{
			foreach (var part in _parts)
			{
				var boundingSphere = part.BoundingSphere.Transform(AbsoluteTransform);
				if (context.Frustrum.Contains(boundingSphere) == ContainmentType.Disjoint)
				{
					continue;
				}

				// If part has bones, then parent node transform had been already
				// applied to bones transform
				// Thus to avoid applying parent transform twice, we use
				// ordinary Transform(not AbsoluteTransform) for parts with bones
				using (var scope = new TransformScope(context,
					part.Bones.Count > 0 ? Matrix.Identity : AbsoluteTransform))
				{
					part.Draw(context);
				}
			}
		}
	}
}