#if UNITY_EDITOR && VRC_SDK_VRCSDK3
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace AvatarScaleSpecify.Editor
{
    internal sealed class ViewPointScalerBuildHook : IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => int.MaxValue - 512;

        public bool OnPreprocessAvatar(GameObject avatarGameObject)
        {
            if (ViewPointScaleProcessor.TryApply(avatarGameObject, out var report))
            {
                Debug.Log($"[ViewPointScaler] {report}");
            }

            return true;
        }
    }
}
#endif
