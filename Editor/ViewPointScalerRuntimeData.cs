using UnityEngine;

namespace AvatarScaleSpecify.Editor
{
    // Internal component to persist data across NDMF phases
    // This component is NOT IEditorOnly, so it survives until we manually destroy it.
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    internal sealed class ViewPointScalerRuntimeData : MonoBehaviour
    {
        public float TargetEyeHeight;
    }
}
