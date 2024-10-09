/*
 * AnimationChannel.cs
 * Author: Bruno Evangelista
 * Copyright (c) 2008 Bruno Evangelista. All rights reserved.
 *
 * THIS SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */
using Nursia.Modelling;
using System;

namespace Nursia.Animation
{
	public class AnimationChannel
	{
		public NursiaModelBone Bone { get; }
		public AnimationChannelKeyframe[] Keyframes { get; }

		public InterpolationMode TranslationMode { get; set; }
		public InterpolationMode RotationMode { get; set; }
		public InterpolationMode ScaleMode { get; set; }

		public AnimationChannel(NursiaModelBone bone, AnimationChannelKeyframe[] keyframes)
		{
			if (bone == null)
			{
				throw new ArgumentNullException(nameof(bone));
			}

			if (keyframes == null)
			{
				throw new ArgumentNullException(nameof(keyframes));
			}

			if (keyframes.Length == 0)
			{
				throw new ArgumentException("no keyframes", nameof(keyframes));
			}

			Bone = bone;
			Keyframes = keyframes;
		}

		/// <summary>
		/// Return the nearest keyframe for the given time
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public int GetKeyframeIndexByTime(TimeSpan time)
		{
			int keyframeIndex = 0;
			int startIndex = 0;
			int endIndex = Keyframes.Length - 1;

			while (endIndex >= startIndex)
			{
				keyframeIndex = (startIndex + endIndex) / 2;

				if (Keyframes[keyframeIndex].Time < time)
					startIndex = keyframeIndex + 1;
				else if (Keyframes[keyframeIndex].Time > time)
					endIndex = keyframeIndex - 1;
				else
					break;
			}

			if (Keyframes[keyframeIndex].Time > time)
				keyframeIndex--;

			if (keyframeIndex < 0)
			{
				keyframeIndex = 0;
			}

			return keyframeIndex;
		}

		public AnimationChannelKeyframe GetKeyframeByTime(TimeSpan time)
		{
			int index = GetKeyframeIndexByTime(time);
			return Keyframes[index];
		}
	}
}