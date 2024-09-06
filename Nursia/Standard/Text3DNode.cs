using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia.Utilities;
using System.IO;

namespace Nursia.Standard
{
	public class Text3DNode : BillboardNodeBase
	{
		private Texture2D _texture = null;
		private string _text;
		private float _fontSize = 32.0f;

		protected internal override Texture2D RenderTexture
		{
			get
			{
				UpdateTexture();

				return _texture;
			}
		}

		public string Text
		{
			get => _text;

			set
			{
				if (value == _text)
				{
					return;
				}

				_text = value;
				Invalidate();
			}
		}

		public float FontSize
		{
			get => _fontSize;

			set
			{
				if (value.EpsilonEquals(_fontSize))
				{
					return;
				}

				_fontSize = value;
				Invalidate();
			}
		}

		private void UpdateTexture()
		{
			if (_texture != null || string.IsNullOrEmpty(_text) || _fontSize.IsZero())
			{
				return;
			}

			var font = Resources.DefaultFontSystem.GetFont(_fontSize);
			var size = font.MeasureString(_text);

			Width = size.X / 64.0f;
			Height = size.Y / 64.0f;

			var device = Nrs.GraphicsDevice;
			var target = new RenderTarget2D(device, (int)size.X, (int)size.Y);

			try
			{
				device.SetRenderTarget(target);

				device.Clear(Color.Black);
				var spriteBatch = Resources.SpriteBatch;
				spriteBatch.Begin();
				spriteBatch.DrawString(font, _text, Vector2.Zero, Color.White);
				spriteBatch.End();
			}
			finally
			{
				device.SetRenderTarget(null);
			}

			_texture = target;

			using (var stream = File.OpenWrite(@"D:\Temp\test.png"))
			{
				_texture.SaveAsPng(stream, _texture.Width, _texture.Height);
			}
		}

		private void Invalidate()
		{
			_texture = null;
		}
	}
}
