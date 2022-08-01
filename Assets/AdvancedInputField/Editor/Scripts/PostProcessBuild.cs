#if UNITY_IOS
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
#endif

namespace AdvancedInputFieldPlugin.Editor
{
    public class PostProcessBuild
    {
#if UNITY_IOS
        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                UnityEngine.Debug.Log("Adding -force_load flag to Other Linker Flags");

                string projectPath = PBXProject.GetPBXProjectPath(path);
                PBXProject project = new PBXProject();
                project.ReadFromFile(projectPath);

#if UNITY_2019_3_OR_NEWER
                string target = project.GetUnityMainTargetGuid();
#else
				string target = project.TargetGuidByName(PBXProject.GetUnityTargetName()); //Use this line for older versions of Unity
#endif
                // NOTE: Had to change this so that the xCode project looks in the correct location to link the library
                // otherwise building fails.
                project.AddBuildProperty(target, "OTHER_LDFLAGS", "-force_load $(PROJECT_DIR)/Libraries/BindyStreet.Kit.Unity/Plugins/AdvancedInputField/Plugins/iOS/NativeKeyboard.a");
                project.WriteToFile(projectPath);
            }
        }
#endif
    }
}

