#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AvatarScaleSpecify;

namespace AvatarScaleSpecify.Editor
{
    internal static class ViewPointScalerMenu
    {
        private const string MenuPath = "VRChat Utility/ViewPoint Scaler";

        [MenuItem(MenuPath, false, priority: 2000)]
        private static void AddComponent()
        {
            foreach (var go in Selection.gameObjects)
            {
                if (go.GetComponent<ViewPointScaler>() != null) continue;

                Undo.RecordObject(go, "Add ViewPointScaler");
                go.AddComponent<ViewPointScaler>();
            }
        }

        [MenuItem(MenuPath, true)]
        private static bool Validate() => Selection.activeGameObject != null;
    }
}
#endif
