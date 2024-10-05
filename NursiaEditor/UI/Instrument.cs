using Nursia.Modelling;

namespace NursiaEditor.UI
{
	public enum InstrumentType
	{
		None,
		RaiseTerrain,
		LowerTerrain,
		Water,
		PaintTexture1,
		PaintTexture2,
		PaintTexture3,
		PaintTexture4,
		Model
	}

	public class Instrument
	{
		public InstrumentType Type { get; set; }

		public float Radius { get; set; } = 4.0f;
		public float Power { get; set; } = 0.2f;
		public NursiaModel Model { get; set; }

		public bool IsPaintInstrument => Type != InstrumentType.None &&
				Type != InstrumentType.Water &&
				Type != InstrumentType.Model;
	}
}
