#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace ArkSharp
{
	public static class AnimatorHelper
	{
		/// <summary>
		/// 按名称查找 Animator Controller 引用的动画片段。
		/// </summary>
		public static AnimationClip FindAnimationClip(this Animator animator, string clipName)
		{
			if (animator == null || string.IsNullOrEmpty(clipName))
				return null;

			var controller = animator.runtimeAnimatorController;
			if (controller == null)
				return null;

			var clips = controller.animationClips;
			for (int i = 0; i < clips.Length; i++)
			{
				var clip = clips[i];
				if (clip != null && clip.name == clipName)
					return clip;
			}

			return null;
		}

		/// <summary>
		/// 在指定动画片段中查找 stringParameter 等于给定值的动画事件。
		/// </summary>
		public static AnimationEvent FindAnimationEvent(this Animator animator, string clipName, string eventName)
		{
			if (string.IsNullOrEmpty(eventName))
				return null;

			var clip = animator.FindAnimationClip(clipName);
			if (clip == null)
				return null;

			var events = clip.events;
			for (int i = 0; i < events.Length; i++)
			{
				var animationEvent = events[i];
				if (animationEvent.stringParameter == eventName)
					return animationEvent;
			}

			return null;
		}

	}
}

#endif
