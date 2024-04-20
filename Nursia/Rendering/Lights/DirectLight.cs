using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Nursia.Rendering.Lights
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
		[JsonIgnore]
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

		protected internal override void Render(RenderContext context)
		{
			base.Render(context);

			context.DirectLights.Add(this);
		}
	}
}
