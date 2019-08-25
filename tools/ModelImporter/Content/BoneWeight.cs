namespace Nursia.ModelImporter.Content
{
	class BoneWeight
	{
		public int VertexId { get; set; }
		public float Weight { get; set; }

		public BoneWeight(int vertexId, float weight)
		{
			VertexId = vertexId;
			Weight = weight;
		}
	}
}
