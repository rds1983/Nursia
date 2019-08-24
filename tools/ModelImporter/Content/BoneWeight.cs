namespace Nursia.ModelImporter.Content
{
	class BoneWeight
	{
		public string Name { get; set; }
		public float Weight { get; set; }

		public BoneWeight(string name, float weight)
		{
			Name = name;
			Weight = weight;
		}
	}
}
