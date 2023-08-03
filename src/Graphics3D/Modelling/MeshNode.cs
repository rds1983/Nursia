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
	}
}