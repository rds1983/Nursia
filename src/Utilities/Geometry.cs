using Microsoft.Xna.Framework;

namespace Nursia.Utilities
{
	public static class Geometry
	{
		internal static Plane CreatePlane(float height,
			Vector3 planeNormalDirection,
			Matrix viewProjection,
			bool clipSide)
		{
			planeNormalDirection.Normalize();
			Vector4 planeCoeffs = new Vector4(planeNormalDirection, height);
			if (clipSide)
				planeCoeffs *= -1;

			Matrix inverseWorldViewProjection = Matrix.Invert(viewProjection);
			inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

			planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
			Plane finalPlane = new Plane(planeCoeffs);

			return finalPlane;
		}
	}
}
