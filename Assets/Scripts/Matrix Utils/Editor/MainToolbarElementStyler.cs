#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MatrixUtils.Extensions;
public static class MainToolbarElementStyler {
	public static void StyleElement<T>(string elementName, System.Action<T> styleAction) where T : VisualElement {
		EditorApplication.delayCall += () => {
			ApplyStyle(elementName, (element) => {
				T targetElement = null;

				if (element is T typedElement) {
					targetElement = typedElement;
				} else {
					targetElement = element.Query<T>().First();
				}

				if (targetElement != null) {
					styleAction(targetElement);
				}
			});
		};
	}

	static void ApplyStyle(string elementName, System.Action<VisualElement> styleCallback) {
		VisualElement element = FindElementByName(elementName);
		if (element != null) {
			styleCallback(element);
		}
	}

	static VisualElement FindElementByName(string name) {
		EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
		foreach (VisualElement root in windows.Select(window => window.rootVisualElement).Where(root => root != null))
		{
			VisualElement element;
			if ((element = root.FindElementByName(name)) != null || (element = root.FindElementByTooltip(name)) != null) return element;
		}
		return null;
	}
}
#endif