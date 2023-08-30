namespace Nursia.Graphics3D.Modelling
{
	public class AnimationTransformKeyframe<T>
	{
		public float Time { get; internal set; }
		public float DeltaK { get; internal set; }
		public T Value { get; }

		public AnimationTransformKeyframe(float time, T value)
		{
			Time = time;
			Value = value;
		}
	}
}