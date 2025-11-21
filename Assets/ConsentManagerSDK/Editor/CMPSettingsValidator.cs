#if UNITY_EDITOR
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

namespace ConsentManagerSDK.Editor
{
    /// <summary>
    /// Ensures CMPSettings is configured before building player artifacts.
    /// </summary>
    internal sealed class CMPSettingsValidator : IPreprocessBuildWithReport
    {
        public int callbackOrder => -100;

        public void OnPreprocessBuild(BuildReport report)
        {
            var settings = CMPSettings.LoadDefault();
            if (settings == null)
            {
                throw new BuildFailedException("CMPSettings asset not found in Resources. Create one via Assets > Create > Consent Manager > CMP Settings.");
            }

            if (!settings.HasValidConfiguration())
            {
                throw new BuildFailedException("CMPSettings still contains placeholder CMP credentials. Update the CMPSettings asset with your CMP ID and domain.");
            }
        }
    }
}
#endif
