#if UNITY_EDITOR
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEngine;

[assembly: ExportsPlugin(typeof(AvatarScaleSpecify.Editor.ViewPointScalerPlugin))]

namespace AvatarScaleSpecify.Editor
{
    internal sealed class ViewPointScalerPlugin : Plugin<ViewPointScalerPlugin>
    {
        public override string QualifiedName => "com.dennoko.vrchat.viewpointscaler";
        public override string DisplayName => "VRC ViewPoint Scaler";

        protected override void Configure()
        {
            // Phase 1: Capture settings before IEditorOnly components are removed
            InPhase(BuildPhase.Resolving)
                .Run(CaptureSettingsPass.Instance);

            // Phase 2: Apply scaling after FloorAdjuster
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.ndmf.floor_adjuster")
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run(ScaleAvatarPass.Instance);
        }
    }

    internal sealed class CaptureSettingsPass : Pass<CaptureSettingsPass>
    {
        public override string DisplayName => "Capture ViewPointScaler settings";

        protected override void Execute(BuildContext context)
        {
            ViewPointScaleProcessor.CaptureSettings(context.AvatarRootObject);
        }
    }

    internal sealed class ScaleAvatarPass : Pass<ScaleAvatarPass>
    {
        public override string DisplayName => "Scale avatar root to target view height";

        protected override void Execute(BuildContext context)
        {
            if (ViewPointScaleProcessor.TryApply(context.AvatarRootObject, out var report))
            {
                Debug.Log($"[ViewPointScaler] {report}");
            }
            else
            {
                // Log the failure reason if it's not just "component missing" (which is common for other avatars)
                // However, since TryApply now logs internally, we might not need this.
                // But let's log it just in case to be sure we see it in the NDMF report window if possible.
                if (!report.Contains("not present"))
                {
                    Debug.LogWarning($"[ViewPointScaler] Skipped: {report}");
                }
            }
        }
    }
}
#endif
