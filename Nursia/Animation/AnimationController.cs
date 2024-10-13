/*
 * AnimationController.cs
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
using Microsoft.Xna.Framework;
using Nursia.Modelling;

namespace Nursia.Animation
{
	/// <summary>
	/// Specifies how an animation clip is played.
	/// </summary>
	public enum PlaybackMode
	{
		/// <summary>
		/// Plays the animation clip in the forward way.
		/// </summary>
		Forward,

		/// <summary>
		/// Plays the animation clip in the backward way.
		/// </summary>
		Backward
	};

	/// <summary>
	/// Controls how animations are played and interpolated.
	/// </summary>
	public class AnimationController
	{
		private TimeSpan _time;
		private float _speed;
		private bool _loopEnabled;
		private AnimationClip _crossFadeAnimationClip;
		private float _crossFadeInterpolationAmount;
		private TimeSpan _crossFadeTime;
		private TimeSpan _crossFadeElapsedTime;

		/// <summary>
		/// Model Node
		/// </summary>
		public NursiaModelNode Node { get; }

		/// <summary>
		/// Gets the animation clip being played.
		/// </summary>
		public AnimationClip AnimationClip { get; private set; }

		/// <summary>
		/// Gets os sets the current animation playback time.
		/// </summary>
		public TimeSpan Time
		{
			get { return _time; }
			set
			{
				if (value == _time)
				{
					return;
				}

				_time = value;
				UpdateAll();

				TimeChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Gets os sets the animation playback speed.
		/// </summary>
		public float Speed
		{
			get { return _speed; }
			set
			{
				if (_speed < 0)
				{
					throw new ArgumentException("Speed must be a positive value");
				}

				_speed = value;
			}
		}

		/// <summary>
		/// Enables animation looping.
		/// </summary>
		public bool LoopEnabled
		{
			get { return _loopEnabled; }
			set
			{
				_loopEnabled = value;

				if (HasFinished && _loopEnabled)
				{
					HasFinished = false;
				}
			}
		}

		/// <summary>
		/// Gets os sets the animation playback mode.
		/// </summary>
		public PlaybackMode PlaybackMode { get; set; }

		/// <summary>
		/// Returns whether the animation has finished.
		/// </summary>
		public bool HasFinished { get; private set; }

		/// <summary>
		/// Returns whether the animation is playing.
		/// </summary>
		public bool IsPlaying { get; private set; }

		public bool CrossFading { get; private set; }

		public int CurrentKeyFrame { get; private set; }

		public event EventHandler TimeChanged;

		/// <summary>Initializes a new instance of the 
		/// <see cref="T:XNAnimation.Controllers.AnimationController" />
		/// class.
		/// </summary>
		/// <param name="skeleton">The skeleton of the model to be animated</param>
		public AnimationController(NursiaModelNode skeleton)
		{
			Node = skeleton ?? throw new ArgumentNullException(nameof(skeleton));
			skeleton.ResetTransforms();

			_time = TimeSpan.Zero;
			_speed = 1.0f;
			_loopEnabled = true;
			PlaybackMode = PlaybackMode.Forward;

			CrossFading = false;
			_crossFadeInterpolationAmount = 0.0f;
			_crossFadeTime = TimeSpan.Zero;
			_crossFadeElapsedTime = TimeSpan.Zero;

			HasFinished = false;
			IsPlaying = false;
		}

		/// <summary>
		/// Starts the playback of an animation clip from the beginning.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		public void StartClip(string name)
		{
			AnimationClip = Node.Model.Animations[name];
			HasFinished = false;
			IsPlaying = true;

			Node.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Stops the playback
		/// </summary>
		public void StopClip()
		{
			AnimationClip = null;
			HasFinished = false;
			IsPlaying = false;

			Node.ResetTransforms();
			Time = TimeSpan.Zero;
		}

		/// <summary>
		/// Plays an animation clip.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		public void PlayClip(string name)
		{
			AnimationClip = Node.Model.Animations[name];

			if (_time < AnimationClip.Duration)
			{
				HasFinished = false;
				IsPlaying = true;
			}
		}

		/// <summary>
		/// Interpolates linearly between two animation clips, fading out the current 
		/// animation clip and fading in a new one.
		/// </summary>
		/// <param name="name">Name of the clip</param>
		/// <param name="fadeTime">Time used to fade in and out the animation clips.</param>
		public void CrossFade(string name, TimeSpan fadeTime)
		{
			if (AnimationClip == null)
			{
				StartClip(name);
				return;
			}

			if (CrossFading)
			{
				StartClip(_crossFadeAnimationClip.Name);
			}

			_crossFadeAnimationClip = Node.Model.Animations[name];
			_crossFadeTime = fadeTime;
			_crossFadeElapsedTime = TimeSpan.Zero;

			CrossFading = true;
		}

		/// <summary>
		/// Updates the animation clip time and calculates the new skeleton's bone pose.
		/// </summary>
		/// <param name="elapsedTime">Time elapsed since the last update.</param>
		public void Update(TimeSpan elapsedTime)
		{
			if (!IsPlaying)
			{
				return;
			}

			if (HasFinished && !CrossFading)
			{
				return;
			}

			// Scale the elapsed time
			TimeSpan scaledElapsedTime = TimeSpan.FromTicks((long)(elapsedTime.Ticks * _speed));
			if (PlaybackMode == PlaybackMode.Backward)
			{
				scaledElapsedTime = -scaledElapsedTime;
			}

			// Adjust controller time
			if (CrossFading)
			{
				_crossFadeElapsedTime += scaledElapsedTime;
			}

			Time += scaledElapsedTime;
		}

		private void UpdateAll()
		{
			if (AnimationClip == null)
			{
				return;
			}

			UpdateAnimationTime();

			if (CrossFading)
			{
				UpdateCrossFadeTime();
			}

			UpdateChannelPoses();

		}

		/// <summary>
		/// Updates the CrossFade time
		/// </summary>
		/// <param name="elapsedTime">Time elapsed since the last update.</param>
		private void UpdateCrossFadeTime()
		{
			if (_crossFadeElapsedTime > _crossFadeTime)
			{
				CrossFading = false;
				_crossFadeInterpolationAmount = 0;
				_crossFadeTime = TimeSpan.Zero;
				_crossFadeElapsedTime = TimeSpan.Zero;

				StartClip(_crossFadeAnimationClip.Name);
			}
			else
				_crossFadeInterpolationAmount = MathHelper.Clamp(_crossFadeElapsedTime.Ticks / (float)_crossFadeTime.Ticks, 0, 1);
		}

		/// <summary>
		/// Updates the animation clip time.
		/// </summary>
		private void UpdateAnimationTime()
		{
			// Animation finished
			if (_time < TimeSpan.Zero || _time > AnimationClip.Duration)
			{
				if (_loopEnabled)
				{
					if (_time > AnimationClip.Duration)
					{
						while (_time > AnimationClip.Duration)
							_time -= AnimationClip.Duration;
					}
					else
					{
						while (_time < TimeSpan.Zero)
							_time += AnimationClip.Duration;
					}

					// Copy bind pose on animation restart
					Node.ResetTransforms();
				}
				else
				{
					_time = (_time > AnimationClip.Duration) ? AnimationClip.Duration : TimeSpan.Zero;

					IsPlaying = false;
					HasFinished = true;
				}
			}
		}

		/// <summary>
		/// Updates the pose of all skeleton's bones.
		/// </summary>
		private void UpdateChannelPoses()
		{
			for (int i = 0; i < AnimationClip.Channels.Length; i++)
			{
				// Search for the current channel in the current animation clip
				var animationChannel = AnimationClip.Channels[i];

				Pose pose;
				InterpolateChannelPose(animationChannel, _time, out pose);
				Node.SetBoneLocalTransform(animationChannel.Bone.Index, pose.ToMatrix());

				// If CrossFade is enabled blend this channel in two animation clips
				if (CrossFading)
				{
					Pose channelPose;

					// Search for the current channel in the cross fade clip
					if (_crossFadeAnimationClip.TryGetChannelByBoneIndex(animationChannel.Bone.Index, out animationChannel))
					{
						InterpolateChannelPose(animationChannel, TimeSpan.Zero, out channelPose);
					}
					else
					{
						channelPose = animationChannel.Bone.DefaultPose;
					}

					// Interpolate each channel with the cross fade animation
					pose = Pose.Interpolate(pose, channelPose, _crossFadeInterpolationAmount,
							animationChannel.TranslationMode, animationChannel.RotationMode, animationChannel.ScaleMode);

					Node.SetBoneLocalTransform(animationChannel.Bone.Index, pose.ToMatrix());
				}
			}
		}

		/// <summary>
		/// Retrieves and interpolates the pose of an animation channel.
		/// </summary>
		/// <param name="animationChannel">Name of the animation channel.</param>
		/// <param name="animationTime">Current animation clip time.</param>
		/// <param name="outPose">The output interpolated pose.</param>
		private void InterpolateChannelPose(AnimationChannel animationChannel, TimeSpan animationTime,
			out Pose outPose)
		{
			if (animationChannel.TranslationMode == InterpolationMode.None &&
				animationChannel.RotationMode == InterpolationMode.None &&
				animationChannel.ScaleMode == InterpolationMode.None)
			{
				CurrentKeyFrame = animationChannel.GetKeyframeIndexByTime(animationTime);
				outPose = animationChannel.Keyframes[CurrentKeyFrame].Pose;
			}
			else
			{
				CurrentKeyFrame = animationChannel.GetKeyframeIndexByTime(animationTime);
				int nextKeyframeIndex;

				// If we are looping then the next frame may wrap around to 
				// the beginning. If not we should just clamp it at the last frame
				if (_loopEnabled)
				{
					nextKeyframeIndex = (CurrentKeyFrame + 1) % animationChannel.Keyframes.Length;
				}
				else
				{
					nextKeyframeIndex = Math.Min(CurrentKeyFrame + 1, animationChannel.Keyframes.Length - 1);
				}

				var keyframe1 = animationChannel.Keyframes[CurrentKeyFrame];
				var keyframe2 = animationChannel.Keyframes[nextKeyframeIndex];

				// Calculate the time between the keyframes considering loop
				long keyframeDuration;
				if (CurrentKeyFrame == (animationChannel.Keyframes.Length - 1))
					keyframeDuration = AnimationClip.Duration.Ticks - keyframe1.Time.Ticks;
				else
					keyframeDuration = keyframe2.Time.Ticks - keyframe1.Time.Ticks;

				// Interpolate when duration higher than zero
				if (keyframeDuration > 0)
				{
					long elapsedKeyframeTime = animationTime.Ticks - keyframe1.Time.Ticks;
					float lerpFactor = MathHelper.Clamp(elapsedKeyframeTime / (float)keyframeDuration, 0, 1);

					outPose = Pose.Interpolate(keyframe1.Pose, keyframe2.Pose, lerpFactor,
								animationChannel.TranslationMode, animationChannel.RotationMode, animationChannel.ScaleMode);
				}
				// Otherwise don't interpolate
				else
					outPose = keyframe1.Pose;
			}
		}
	}
}
