using MatrixUtils.DependencyInjection;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
namespace MatrixUtils.DependencyInjection {
    [CustomEditor(typeof(Injector))]
    public class InjectorEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            Injector injector = (Injector) target;

            if (GUILayout.Button("Validate Dependencies")) {
                Injector.ValidateDependencies();
            }

            if (!GUILayout.Button("Clear All Injectable Fields")) return;
            Injector.ClearDependencies();
            EditorUtility.SetDirty(injector);
        }
    }
}
#endif