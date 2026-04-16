#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
[UsedImplicitly]
public class MainToolbarProjectButtons
{
	[MainToolbarElement("Project/Open Project Settings", defaultDockPosition = MainToolbarDockPosition.Right), UsedImplicitly]
	public static MainToolbarElement ProjectSettingsButton()
	{
		Texture2D icon = EditorGUIUtility.IconContent("SettingsIcon").image as Texture2D;
		MainToolbarContent content = new(icon, "Open Project Settings");
		return new MainToolbarButton(content, () => { SettingsService.OpenProjectSettings(); });
	}
	[MainToolbarElement("Project/Open Lighting", defaultDockPosition = MainToolbarDockPosition.Right), UsedImplicitly]
	public static MainToolbarElement LightingButton()
	{
		Texture2D icon = EditorGUIUtility.IconContent("Lighting").image as Texture2D;
		MainToolbarContent content = new(icon, "Open Lighting");
		return new MainToolbarButton(content, () => {
			EditorWindow.GetWindow(System.Type.GetType("UnityEditor.LightingWindow,UnityEditor"));
		});
	}
}
#endif