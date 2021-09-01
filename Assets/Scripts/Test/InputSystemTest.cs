using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemTest : MonoBehaviour
{
    [SerializeField] int m_fontSize = 10;

    void Update()
    {
        // ゲームパッドが接続されていないとnullになる。
        if (Gamepad.current == null) return;

        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            Debug.Log("Button Northが押された！");
        }
        if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
        {
            Debug.Log("Button Southが離された！");
        }
    }

    void OnGUI()
    {
        GUI.skin.label.fontSize = m_fontSize;

        if (Gamepad.current != null)
        {
            GUILayout.BeginArea(new Rect(Screen.width / 2 - Screen.width / 4, Screen.height / 2 - Screen.height / 4, Screen.width, Screen.height));

            GUILayout.Label($"leftStick: {Gamepad.current.leftStick.ReadValue()}");
            GUILayout.Label($"rightStick: {Gamepad.current.rightStick.ReadValue()}");

            GUILayout.Label($"buttonNorth: {Gamepad.current.buttonNorth.isPressed}");
            GUILayout.Label($"buttonSouth: {Gamepad.current.buttonSouth.isPressed}");
            GUILayout.Label($"buttonEast: {Gamepad.current.buttonEast.isPressed}");
            GUILayout.Label($"buttonWest: {Gamepad.current.buttonWest.isPressed}");

            GUILayout.Label($"buttonUp: {Gamepad.current.dpad.up.isPressed}");
            GUILayout.Label($"buttonDown: {Gamepad.current.dpad.down.isPressed}");
            GUILayout.Label($"buttonRight: {Gamepad.current.dpad.right.isPressed}");
            GUILayout.Label($"buttonLeft: {Gamepad.current.dpad.left.isPressed}");

            GUILayout.Label($"leftShoulder: {Gamepad.current.leftShoulder.ReadValue()}");
            GUILayout.Label($"leftTrigger: {Gamepad.current.leftTrigger.ReadValue()}");

            GUILayout.Label($"rightShoulder: {Gamepad.current.rightShoulder.ReadValue()}");
            GUILayout.Label($"rightTrigger: {Gamepad.current.rightTrigger.ReadValue()}");

            GUILayout.EndArea();
        }
        else if (Keyboard.current != null && Mouse.current != null)
        {
            GUILayout.Label($"anyKey: {Keyboard.current.anyKey.ReadValue()}");

            GUILayout.Label($"leftClick: {Mouse.current.leftButton.ReadValue()}");
            GUILayout.Label($"rightClick: {Mouse.current.rightButton.ReadValue()}");
        }
    }
}
