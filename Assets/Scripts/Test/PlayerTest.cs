using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerTest : MonoBehaviour
{
    Vector3 move;

    public void OnMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            Gamepad.current.SetMotorSpeeds(0.1f, 0.1f);
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
        const float Speed = 1f;
        transform.Translate(move * Speed * Time.deltaTime);
    }
}
