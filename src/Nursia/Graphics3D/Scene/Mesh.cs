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

		public Bone RootBone { get; set; }

		public Matrix Transform
		{
			get
			{
				return RootBone.Transform;
			}

			set
			{
				RootBone.Transform = value;
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