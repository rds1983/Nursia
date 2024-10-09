/*
 * AnimationClip.cs
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
using System;
using System.Collections.Generic;

namespace Nursia.Animation
{
	public class AnimationClip
	{
		private Dictionary<int, AnimationChannel> _channelsByBones = new Dictionary<int, AnimationChannel>();

		public string Name { get; }

		public TimeSpan Duration { get; }
		public AnimationChannel[] Channels { get; }

		public AnimationClip(string name, TimeSpan duration, AnimationChannel[] channels)
		{
			if (channels == null)
			{
				throw new ArgumentNullException(nameof(channels));
			}

			if (channels.Length == 0)
			{
				throw new ArgumentException("no channels", nameof(channels));
			}

			Name = name;
			Duration = duration;
			Channels = channels;

			foreach (var channel in channels)
			{
				_channelsByBones[channel.Bone.Index] = channel;
			}
		}

		public bool TryGetChannelByBoneIndex(int boneIndex, out AnimationChannel result)
		{
			return _channelsByBones.TryGetValue(boneIndex, out result);
		}
	}
}