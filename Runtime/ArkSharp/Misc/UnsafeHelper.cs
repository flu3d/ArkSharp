using System.Runtime.CompilerServices;

namespace ArkSharp
{
    public static class UnsafeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SizeOf<T>() where T : unmanaged
        {
            unsafe
            {
                return sizeof(T);
            }
        }
    }
}
