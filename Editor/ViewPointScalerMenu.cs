#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using AvatarScaleSpecify;

namespace AvatarScaleSpecify.Editor
{
    internal static class ViewPointScalerMenu
    {
        private const string MenuPath = "dennokoworks/ViewPoint Scaler";
        private const string ChildMenuPath = "GameObject/dennokoworks/Add ViewPoint Scaler Child";

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

        [MenuItem(ChildMenuPath, false, priority: 10)]
        private static void AddChildObject()
        {
            var selections = Selection.transforms;
            if (selections == null || selections.Length == 0) return;

            foreach (var parent in selections)
            {
                if (parent == null) continue;

                var child = new GameObject("AvatarScaler");
                Undo.RegisterCreatedObjectUndo(child, "Create ViewPoint Scaler Child");
                child.transform.SetParent(parent, false);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = Vector3.one;
                GameObjectUtility.EnsureUniqueNameForSibling(child);

                Undo.AddComponent<ViewPointScaler>(child);
                Selection.activeGameObject = child;
            }
        }

        [MenuItem(ChildMenuPath, true)]
        private static bool ValidateChildObject() => Selection.activeTransform != null;
    }
}
#endif
