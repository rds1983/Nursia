using Microsoft.Xna.Framework;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Nursia.Graphics3D.Lights
{
	public class DirectLight : BaseLight
	{
		private Vector3 _direction;
		private Vector3 _normalizedDirection;

		public Vector3 Direction
		{
			get { return _direction; }
			set
			{
				_direction = value;
				UpdateNormalizedDirection();
			}
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Vector3 NormalizedDirection
		{
			get { return _normalizedDirection; }
		}

		private void UpdateNormalizedDirection()
		{
			var length = _direction.Length();
			if (length < 0.00001f)
			{
				_normalizedDirection = Vector3.Zero;
			}
			else
			{
				_normalizedDirection = _direction;
				_normalizedDirection.Normalize();
			}

		}
	}
}