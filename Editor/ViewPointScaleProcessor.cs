#if UNITY_EDITOR
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using AvatarScaleSpecify;
using Object = UnityEngine.Object;

namespace AvatarScaleSpecify.Editor
{
    internal static class ViewPointScaleProcessor
    {
        private const float MinHeight = 0.01f;

        internal static void CaptureSettings(GameObject avatarRoot)
        {
            if (avatarRoot == null) return;
            
            var scaler = FindScaler(avatarRoot);
            if (scaler == null) return;

            // Create runtime data holder
            var data = avatarRoot.GetComponent<ViewPointScalerRuntimeData>();
            if (data == null)
            {
                data = avatarRoot.AddComponent<ViewPointScalerRuntimeData>();
            }

            if (data == null)
            {
                Debug.LogError("[ViewPointScaler] Failed to create ViewPointScalerRuntimeData component. This may be due to script compilation issues or assembly definitions.");
                return;
            }

            data.TargetEyeHeight = scaler.TargetEyeHeight;
            
            // We don't destroy the original scaler here; let IEditorOnly or CleanupScalers handle it.
            Debug.Log($"[ViewPointScaler] Captured target height: {data.TargetEyeHeight}");
        }

        internal static bool TryApply(GameObject avatarRoot, out string report)
        {
            report = string.Empty;
            Debug.Log($"[ViewPointScaler] Starting processing for {avatarRoot.name}");

            if (avatarRoot == null)
            {
                report = "Avatar root was null.";
                Debug.LogError($"[ViewPointScaler] {report}");
                return false;
            }

            var descriptor = FindDescriptor(avatarRoot);
            if (descriptor == null)
            {
                report = "No VRCAvatarDescriptor found on avatar root.";
                Debug.LogError($"[ViewPointScaler] {report}");
                return false;
            }

            // Try to find runtime data first (preferred), then fallback to original component
            float targetHeight = -1f;
            var runtimeData = avatarRoot.GetComponent<ViewPointScalerRuntimeData>();
            
            if (runtimeData != null)
            {
                targetHeight = runtimeData.TargetEyeHeight;
                Debug.Log($"[ViewPointScaler] Using captured runtime data: {targetHeight}");
            }
            else
            {
                var scaler = FindScaler(avatarRoot);
                if (scaler != null)
                {
                    targetHeight = scaler.TargetEyeHeight;
                    Debug.Log($"[ViewPointScaler] Using component directly: {targetHeight}");
                }
            }

            if (targetHeight < 0)
            {
                report = "ViewPointScaler component (or captured data) not present.";
                Debug.LogWarning($"[ViewPointScaler] {report} (This is normal if the component was not added to this avatar)");
                return false;
            }

            // DEBUG: Force target height to 1.8m for verification
            // Debug.LogWarning("[ViewPointScaler] DEBUG MODE: Forcing target height to 1.8m to verify ViewPosition update.");
            // targetHeight = 1.8f;

            targetHeight = Mathf.Max(MinHeight, targetHeight);
            Debug.Log($"[ViewPointScaler] Target Height: {targetHeight}");

            var currentHeight = MeasureViewHeight(descriptor, avatarRoot.transform);
            Debug.Log($"[ViewPointScaler] Measured Current Height: {currentHeight}");

            if (currentHeight <= MinHeight)
            {
                CleanupScalers(avatarRoot);
                report = "Unable to evaluate current view height (too small).";
                return false;
            }

            var scaleFactor = targetHeight / currentHeight;
            Debug.Log($"[ViewPointScaler] Calculated Scale Factor: {scaleFactor}");

            var oldScale = avatarRoot.transform.localScale;
            avatarRoot.transform.localScale *= scaleFactor;
            Debug.Log($"[ViewPointScaler] Applied Scale: {oldScale} -> {avatarRoot.transform.localScale}");

            UpdateDescriptorView(descriptor, scaleFactor);
            CleanupScalers(avatarRoot);

            report = $"Scaled avatar by {scaleFactor:F3}x ({currentHeight:F3}m -> {targetHeight:F3}m).";
            return true;
        }

        private static void CleanupScalers(GameObject avatarRoot)
        {
            foreach (var marker in avatarRoot.GetComponentsInChildren<ViewPointScaler>(true))
            {
                Object.DestroyImmediate(marker);
            }
            foreach (var data in avatarRoot.GetComponentsInChildren<ViewPointScalerRuntimeData>(true))
            {
                Object.DestroyImmediate(data);
            }
        }

        private static float MeasureViewHeight(VRCAvatarDescriptor descriptor, Transform avatarRoot)
        {
            var viewWorld = descriptor.transform.TransformPoint(descriptor.ViewPosition);
            var ground = avatarRoot.position.y;
            return Mathf.Max(MinHeight, viewWorld.y - ground);
        }

        private static void UpdateDescriptorView(VRCAvatarDescriptor descriptor, float scaleFactor)
        {
            // FloorAdjuster (by scale) also tweaks ViewPosition.y/z, and VRChat does not propagate
            // root scale into the descriptor values. To stay consistent we scale only Y/Z directly.
            var oldView = descriptor.ViewPosition;
            var newView = new Vector3(
                oldView.x,
                oldView.y * scaleFactor,
                oldView.z * scaleFactor);

            Debug.Log($"[ViewPointScaler] Updating ViewPosition (Direct Y/Z Scale): {oldView} -> {newView}");
            descriptor.ViewPosition = newView;

            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(descriptor);
            }
        }

        private static VRCAvatarDescriptor FindDescriptor(GameObject avatarRoot)
        {
            var direct = avatarRoot.GetComponent<VRCAvatarDescriptor>();
            if (direct != null) return direct;
            var descriptors = avatarRoot.GetComponentsInChildren<VRCAvatarDescriptor>(true);
            return descriptors.Length > 0 ? descriptors[0] : null;
        }

        private static ViewPointScaler FindScaler(GameObject avatarRoot)
        {
            var atRoot = avatarRoot.GetComponent<ViewPointScaler>();
            if (atRoot != null) return atRoot;
            var scalers = avatarRoot.GetComponentsInChildren<ViewPointScaler>(true);
            return scalers.Length > 0 ? scalers[0] : null;
        }
    }
}
#endif
