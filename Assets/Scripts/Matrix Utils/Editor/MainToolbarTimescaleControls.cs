#if UNITY_EDITOR
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

[UsedImplicitly]
public class MainToolbarTimescaleControls
{
	const float MinTimeScale = 0f;
	const float MaxTimeScale = 5f;

	[MainToolbarElement("Timescale/Slider", defaultDockPosition = MainToolbarDockPosition.Middle), UsedImplicitly]
	public static MainToolbarElement TimeSlider() {
		MainToolbarContent content = new("Time Scale", "Change the game's time scale");
		MainToolbarSlider slider = new(content, Time.timeScale, MinTimeScale, MaxTimeScale, OnSliderValueChanged)
		{
			populateContextMenu = menu => {
				menu.AppendAction("Reset", _ => {
					Time.timeScale = 1f;
					MainToolbar.Refresh("Timescale/Slider");
				});
			}
		};
		MainToolbarElementStyler.StyleElement<VisualElement>("Timescale/Slider", element =>
		{
			element.style.paddingLeft = 10f;
		});
		return slider;
	}

	static void OnSliderValueChanged(float newValue) {
		Time.timeScale = newValue;
	}

	[MainToolbarElement("Timescale/Reset", defaultDockPosition = MainToolbarDockPosition.Middle), UsedImplicitly]
	public static MainToolbarElement ResetTimeScaleButton() {
		Texture2D icon = EditorGUIUtility.IconContent("Refresh").image as Texture2D;
		MainToolbarContent content = new(icon, "Reset");
		MainToolbarButton button = new(content, () => {
			Time.timeScale = 1f;
			MainToolbar.Refresh("Timescale/Slider");
		});

		MainToolbarElementStyler.StyleElement<EditorToolbarButton>("Timescale/Reset", element => {
			element.style.paddingLeft = 0f;
			element.style.paddingRight = 0f;
			element.style.marginLeft = 0f;
			element.style.marginRight = 0f;
			element.style.minWidth = 20f;
			element.style.maxWidth = 20f;

			Image image = element.Q<Image>();
			if (image == null) return;
			image.style.width = 12f;
			image.style.height = 12f;
		});

		return button;
	}
}
#endif