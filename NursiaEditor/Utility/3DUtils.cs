using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Modelling;
using Nursia.Rendering;
using Nursia.Rendering.Lights;
using Nursia.Rendering.Vertices;
using Nursia.Standard;
using Nursia.Utility;

namespace NursiaEditor.Utility
{
	internal static class _3DUtils
	{
		private static Mesh _selectionMesh;
		private static EditorNode _selectionNode;

		private static Mesh SelectionMesh
		{
			get
			{
				if (_selectionMesh == null)
				{
					var vertices = new VertexPosition[]
					{
						new VertexPosition(new Vector3(-0.5f, 0.5f, 0.5f)),
						new VertexPosition(new Vector3(0.5f, 0.5f, 0.5f)),
						new VertexPosition(new Vector3(0.5f, -0.5f, 0.5f)),
						new VertexPosition(new Vector3(-0.5f, -0.5f, 0.5f)),
						new VertexPosition(new Vector3(-0.5f, 0.5f, -0.5f)),
						new VertexPosition(new Vector3(0.5f, 0.5f, -0.5f)),
						new VertexPosition(new Vector3(0.5f, -0.5f, -0.5f)),
						new VertexPosition(new Vector3(-0.5f, -0.5f, -0.5f))
					};

					var indices = new short[]
					{
						0, 1,
						1, 2,
						2, 3,
						3, 0,

						4, 5,
						5, 6,
						6, 7,
						7, 4,

						0, 4,
						1, 5,
						2, 6,
						3, 7,

						3, 1,
						5, 2,
						7, 0,
						6, 4,
						1, 4,
						3, 6
					};

					_selectionMesh = new Mesh(vertices, indices, PrimitiveType.LineList);
				}

				return _selectionMesh;
			}
		}

		private static EditorNode SelectionNode
		{
			get
			{
				if (_selectionNode == null)
				{
					_selectionNode = new EditorNode(
						SelectionMesh,
						new ColorMaterial
						{
							Color = Color.Orange
						});
				}

				return _selectionNode;
			}
		}


		public static BoundingBox? GetPickBox(this object obj)
		{
			BoundingBox? result = null;

			var asMeshNode = obj as MeshNodeBase;
			if (asMeshNode != null)
			{
				result = asMeshNode.BoundingBox;
			}

			if (obj is BaseLight || obj is Camera)
			{
				result = MathUtils.CreateBoundingBox(
					0, 1,
					0, 1,
					0, 1);
			}

			var asModel = obj as NursiaModel;
			if (asModel != null)
			{
				result = asModel.Model.BoundingBox;
			}

			return result;
		}

		public static EditorNode GetSelectionNode(this SceneNode node)
		{
			if (node == null)
			{
				return null;
			}

			var box = node.GetPickBox();
			if (box == null)
			{
				return null;
			}

			var result = SelectionNode;

			var scale = Matrix.CreateScale(box.Value.ToScale());
			result.Transform = scale * node.GlobalTransform;

			return result;
		}
	}
}