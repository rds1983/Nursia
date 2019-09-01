using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Nursia.Graphics3D.Scene
{
	public class Node: ItemWithId
	{
		private readonly List<Node> _children = new List<Node>();

		public Node Parent { get; set; }

		public Matrix DefaultTransform { get; set; } = Matrix.Identity;

		public Matrix Transform { get; set; } = Matrix.Identity;

		internal Matrix AbsoluteTransform { get; set; } = Matrix.Identity;

		public List<Node> Children
		{
			get
			{
				return _children;
			}
		}

		internal Node FindNodeById(string id)
		{
			if (Id == id)
			{
				return this;
			}

			foreach (var child in Children)
			{
				var result = child.FindNodeById(id);
				if (result != null)
				{
					return result;
				}
			}

			return null;
		}

		internal void UpdateAbsoluteTransforms(Matrix rootTransform)
		{
			AbsoluteTransform = Transform * rootTransform;
			foreach (var child in Children)
			{
				child.UpdateAbsoluteTransforms(AbsoluteTransform);
			}
		}
	}
}
