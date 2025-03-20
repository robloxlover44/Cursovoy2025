using System.Runtime.InteropServices;
using UnityEngine;

public class MouseCenteringExample : MonoBehaviour {

#if UNITY_STANDALONE_WIN

	[DllImport("user32.dll")]
	static extern bool SetCursorPos(int X, int Y);

	private void Start() {
		var center = Screen.safeArea.center;
		SetCursorPos((int)center.x, (int)center.y);
	}

#endif

}