using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerTest : MonoBehaviour
{
    const float Speed = 1f;
    Vector3 move;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (Gamepad.current == null) return;

        if (context.phase == InputActionPhase.Started)
        {
            Gamepad.current.SetMotorSpeeds(0.2f, 0.3f);
            Debug.Log($"Fire!");
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            Gamepad.current.SetMotorSpeeds(0, 0);
            Debug.Log($"UnFire!");
        }
    }

    void Update()
    {
        transform.Translate(move * Speed * Time.deltaTime);
    }
}
