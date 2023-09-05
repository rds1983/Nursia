namespace Nursia.Samples.LevelEditor.UI
{
	public enum InstrumentType
	{
		None,
		RaiseTerrain,
		LowerTerrain
	}

	public class Instrument
	{
		public InstrumentType Type { get; set; }

		public float Power { get; set; } = 4.0f;
	}
}
