namespace Nursia.Animation
{
	public class AnimationTransformKeyframe
	{
		public float Time { get; }
		public Pose Pose { get; }

		public AnimationTransformKeyframe(float time, Pose pose)
		{
			Time = time;
			Pose = pose;
		}
	}
}