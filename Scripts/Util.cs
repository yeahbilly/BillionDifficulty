using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace BillionDifficulty;

public class Util {
	public static bool IsDifficulty(int difficulty) {
		return PrefsManager.Instance.GetInt("difficulty") == difficulty;
	}

	public static int GetDifficulty() {
		return PrefsManager.Instance.GetInt("difficulty");
	}

	public static bool IsHardMode() {
		return Plugin.IsBrilliantBillion.Value && IsDifficulty(19);
	}

	public static T[] AddToArray<T>(ref T[] array, T element, int index = -1) {
		if (index == -1) {
			index = array.Length;
		}
		
		List<T> list = array.ToList();
		list.Insert(index, element);
		array = list.ToArray();
		return array;
	}

	public static T[] AddToArray<T>(ref T[] array, T[] elements, int index = -1) {
		if (index == -1) {
			index = array.Length;
		}
		int elementIndex = 0;
		int insertIndex = index;
		List<T> list = array.ToList();
		while (elementIndex < elements.Length) {
			list.Insert(insertIndex, elements[elementIndex]);
			elementIndex++;
			insertIndex++;
		}
		array = list.ToArray();
		return array;
	}

	public static Texture2D LoadEmbeddedTexture(string resourceName) {
		Assembly assembly = Assembly.GetExecutingAssembly();
		using Stream stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null) {
			Plugin.Logger.LogError($"Embedded resource '{resourceName}' not found");
			return null;
		}

		byte[] buffer = new byte[stream.Length];
		stream.Read(buffer, 0, buffer.Length);
		Texture2D tex = new Texture2D(2, 2);
		if (tex.LoadImage(buffer)) {
			return tex;
		} else {
			Plugin.Logger.LogError("Failed to load embedded texture");
			return null;
		}
	}
}
