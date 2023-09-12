using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class ModelNode : ItemWithId
	{
		public List<Mesh> Meshes { get; } = new List<Mesh>();

		public Skin Skin { get; set; }

		public Vector3 DefaultTranslation { get; set; } = Vector3.Zero;
		public Vector3 DefaultScale { get; set; } = Vector3.One;
		public Quaternion DefaultRotation { get; set; } = Quaternion.Identity;

		public int? ParentIndex;
		public List<int> ChildrenIndices { get; } = new List<int>();
	}
}
