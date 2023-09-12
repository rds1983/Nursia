using Microsoft.Xna.Framework;
using Nursia.Utilities;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Modelling
{
	public class NursiaModel
	{
		public List<ModelNode> AllNodes { get; } = new List<ModelNode>();

		public List<ModelNode> RootNodes { get; } = new List<ModelNode>();

		public Dictionary<string, ModelAnimation> Animations { get; } = new Dictionary<string, ModelAnimation>();

		internal NursiaModel()
		{
		}

		public ModelInstance CreateInstance()
		{
			var result = new ModelInstance(this);

			result.ResetTransforms();
			result.UpdateNodesAbsoluteTransforms();

			var boundingBox = new BoundingBox();
			result.TraverseNodes(n =>
			{
				var m = n.AbsoluteTransform;
				foreach (var mesh in n.Node.Meshes)
				{
					var bb = mesh.BoundingBox.Transform(ref m);
					boundingBox = BoundingBox.CreateMerged(boundingBox, bb);
				}
			});

			result.BoundingBox = boundingBox;

			return result;
		}
	}
}