using UnityEngine;
using VRC.SDKBase;

namespace AvatarScaleSpecify
{
    [DisallowMultipleComponent]
    [AddComponentMenu("dennokoworks/ViewPoint Scaler")]
    [ExecuteAlways]
    public sealed class ViewPointScaler : MonoBehaviour, IEditorOnly
    {
        private const float MinHeightMeters = 0.2f;

        [Tooltip("Desired eye height in meters after FloorAdjuster has grounded the avatar.")]
        [Min(MinHeightMeters)]
        public float targetEyeHeight = 1.6f;

        public float TargetEyeHeight => Mathf.Max(MinHeightMeters, targetEyeHeight);

        private void Reset()
        {
            targetEyeHeight = Mathf.Clamp(targetEyeHeight, MinHeightMeters, 3.0f);
        }

        private void OnValidate()
        {
            targetEyeHeight = Mathf.Max(MinHeightMeters, targetEyeHeight);

            if (gameObject.CompareTag("EditorOnly"))
            {
                Debug.LogWarning(
                    "[ViewPointScaler] アバタールートにEditorOnlyタグが付いているとビルド時にアバター全体が削除されます。タグをDefault等に戻してください。",
                    this);
            }
        }

    }
}
