#if UNITY_5_3_OR_NEWER

using UnityEngine;

namespace Ark
{
	public class DontDestroy : MonoBehaviour
	{
		void Awake()
		{
			DontDestroyOnLoad(gameObject);
			Destroy(this);
		}
	}
}

#endif
