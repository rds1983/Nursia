using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace Nursia
{
	public class InputEventArgs<T> : EventArgs
	{
		public T OldValue { get; }
		public T NewValue { get; }

		public InputEventArgs(T oldValue, T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}

	public class KeyEventsArgs : EventArgs
	{
		public Keys Key { get; }

		public KeyEventsArgs(Keys key)
		{
			Key = key;
		}
	}

	public class InputService
	{
		private bool _mouseSet;
		private readonly bool[] _keysDown = new bool[256];
		public Point MousePosition { get; private set; }

		public event EventHandler<InputEventArgs<Point>> MouseMoved;
		public event EventHandler<KeyEventsArgs> KeyDown;
		public event EventHandler<KeyEventsArgs> KeyUp;

		public bool IsKeyDown(Keys key) => _keysDown[(int)key];

		public void Update()
		{
			var mouseState = Mouse.GetState();
			var newPosition = new Point(mouseState.X, mouseState.Y);
			if (!_mouseSet)
			{
				MousePosition = newPosition;
				_mouseSet = true;
			}
			else
			{
				var moved = false;
				var oldPosition = MousePosition;
				if (newPosition != oldPosition)
				{
					moved = true;
				}

				MousePosition = newPosition;
				if (moved)
				{
					MouseMoved?.Invoke(this, new InputEventArgs<Point>(oldPosition, newPosition));
				}
			}

			var keyboardState = Keyboard.GetState();
			for (var i = 0; i < _keysDown.Length; ++i)
			{
				var key = (Keys)i;
				var isDown = keyboardState.IsKeyDown(key);

				if (!_keysDown[i] && isDown)
				{
					KeyDown?.Invoke(this, new KeyEventsArgs(key));
				}
				else if (_keysDown[i] && !isDown)
				{
					KeyUp?.Invoke(this, new KeyEventsArgs(key));
				}

				_keysDown[i] = isDown;
			}
		}
	}
}
