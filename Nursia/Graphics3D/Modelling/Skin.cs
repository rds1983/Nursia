using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nursia.Graphics3D.Modelling
{
	public class Skin: ItemWithId
	{
		public List<int> JointIndices { get; } = new List<int>();
		public Matrix[] Transforms { get; set; }
	}
}