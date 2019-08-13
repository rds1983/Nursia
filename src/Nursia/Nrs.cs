using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Reflection;

namespace Nursia
{
	public static class Nrs
	{
		private static Game _game;

		public static Game Game
		{
			get
			{
				return _game;
			}

			set
			{
				if (_game == value)
				{
					return;
				}

				if (_game != null)
				{
					_game.Disposed -= GameOnDisposed;
				}

				_game = value;

				if (_game != null)
				{
					_game.Disposed += GameOnDisposed;
				}
			}
		}

		public static GraphicsDevice GraphicsDevice
		{
			get
			{
				return Game.GraphicsDevice;
			}
		}

		public static Action<string> InfoLogHandler = Console.WriteLine;
		public static Action<string> WarnLogHandler = Console.WriteLine;
		public static Action<string> ErrorLogHandler = Console.WriteLine;

		public static string Version
		{
			get
			{
				var assembly = typeof(Nrs).GetTypeInfo().Assembly;
				var name = new AssemblyName(assembly.FullName);

				return name.Version.ToString();
			}
		}

#if MONOGAME
		private static bool? _isOpenGL;
		public static bool IsOpenGL
		{
			get
			{
				if (_isOpenGL == null)
				{
					_isOpenGL = (from f in typeof(GraphicsDevice).GetFields(BindingFlags.NonPublic |
						 BindingFlags.Instance)
								 where f.Name == "glFramebuffer"
								 select f).FirstOrDefault() != null;
				}

				return _isOpenGL.Value;
			}
		}
#endif

		private static string FormatMessage(string message, params object[] args)
		{
			string str;
			try
			{
				if (args != null && args.Length > 0)
				{
					str = string.Format(message, args);
				}
				else
				{
					str = message;
				}
			}
			catch (FormatException)
			{
				str = message;
			}

			return str;
		}

		public static void LogInfo(string message, params object[] args)
		{
			if (InfoLogHandler == null)
			{
				return;
			}

			InfoLogHandler(FormatMessage(message, args));
		}

		public static void LogWarn(string message, params object[] args)
		{
			if (WarnLogHandler == null)
			{
				return;
			}

			WarnLogHandler(FormatMessage(message, args));
		}

		public static void LogError(string message, params object[] args)
		{
			if (ErrorLogHandler == null)
			{
				return;
			}

			ErrorLogHandler(FormatMessage(message, args));
		}

		private static void GameOnDisposed(object sender, EventArgs eventArgs)
		{
		}
	}
}