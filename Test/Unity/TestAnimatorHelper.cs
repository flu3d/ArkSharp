#if UNITY_5_3_OR_NEWER

using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace ArkSharp.Test.Unity
{
	[TestFixture]
	public class TestAnimatorHelper
	{
		private const string CLIP_NAME = "TestClip";
		private const string STATE_NAME = "DifferentStateName";
		private const string EVENT_NAME = "TestEvent";

		private GameObject _gameObject;
		private Animator _animator;
		private AnimatorController _controller;
		private AnimationClip _clip;

		[SetUp]
		public void SetUp()
		{
			_gameObject = new GameObject(nameof(TestAnimatorHelper));
			_animator = _gameObject.AddComponent<Animator>();

			_controller = new AnimatorController();
			_controller.AddLayer("Base Layer");

			_clip = new AnimationClip { name = CLIP_NAME };
			var state = _controller.layers[0].stateMachine.AddState(STATE_NAME);
			state.motion = _clip;

			_animator.runtimeAnimatorController = _controller;
		}

		[TearDown]
		public void TearDown()
		{
			Object.DestroyImmediate(_gameObject);
			Object.DestroyImmediate(_clip);
			Object.DestroyImmediate(_controller);
		}

		[Test]
		public void TestFindAnimationClipWithDifferentStateName()
		{
			Assert.AreSame(_clip, _animator.FindAnimationClip(CLIP_NAME));
			Assert.IsNull(_animator.FindAnimationClip(STATE_NAME));
		}

		[Test]
		public void TestFindAnimationClipWithInvalidArguments()
		{
			Assert.IsNull(((Animator)null).FindAnimationClip(CLIP_NAME));
			Assert.IsNull(_animator.FindAnimationClip(null));
			Assert.IsNull(_animator.FindAnimationClip(string.Empty));
			Assert.IsNull(_animator.FindAnimationClip("MissingClip"));
		}

		[Test]
		public void TestFindAnimationEventByStringParameter()
		{
			_clip.AddEvent(new AnimationEvent
			{
				functionName = "DispatchAnimationEvent",
				stringParameter = EVENT_NAME,
			});

			var animationEvent = _animator.FindAnimationEvent(CLIP_NAME, EVENT_NAME);

			Assert.NotNull(animationEvent);
			Assert.AreEqual("DispatchAnimationEvent", animationEvent.functionName);
			Assert.IsNull(_animator.FindAnimationEvent(CLIP_NAME, "MissingEvent"));
		}

		[Test]
		public void TestFindAnimationEventWithInvalidArguments()
		{
			Assert.IsNull(((Animator)null).FindAnimationEvent(CLIP_NAME, EVENT_NAME));
			Assert.IsNull(_animator.FindAnimationEvent(null, EVENT_NAME));
			Assert.IsNull(_animator.FindAnimationEvent(CLIP_NAME, null));
			Assert.IsNull(_animator.FindAnimationEvent(CLIP_NAME, string.Empty));
		}

	}
}

#endif
