using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IIsCanMove
{
    [SerializeField] int m_sensitivity = 1;
    [SerializeField] float m_mouseYMaxRange = 300f;
    [SerializeField] float m_mouseYMinRange = 0f;
    bool m_canMove = true;

    public int Sensitivity
    {
        get => m_sensitivity;
        set
        {
            if (value < 0)
            {
                m_sensitivity = 0;
            }
            else if (value > 100)
            {
                m_sensitivity = 100;
            }
            else
            {
                m_sensitivity = value;
            }
        }
    }

    private void Start()
    {
        MenuController.IsMove += IsMove;
        SensitivityController.SetYSensitivity += SetYSensitivity;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_canMove)
        {
            CameraControlle();
        }
    }

    private void OnDestroy()
    {
        MenuController.IsMove -= IsMove;
        SensitivityController.SetYSensitivity -= SetYSensitivity;
    }

    public void IsMove(bool isMove) => m_canMove = isMove;

    void SetYSensitivity(int value) => Sensitivity = value;

    void CameraControlle()
    {
        float mouse_move_y = (-Input.GetAxis("Look Y") * m_sensitivity) * Time.deltaTime;

        transform.Rotate(new Vector3(-mouse_move_y, 0f, 0f));

        if (transform.localEulerAngles.x < m_mouseYMaxRange && transform.localEulerAngles.x > 90)
        {
            Vector3 v3 = transform.localEulerAngles;
            v3.x = m_mouseYMaxRange;
            transform.localEulerAngles = v3;
        }

        if (transform.localEulerAngles.x > m_mouseYMinRange && transform.localEulerAngles.x < 90)
        {
            Vector3 v3 = transform.localEulerAngles;
            v3.x = m_mouseYMinRange;
            transform.localEulerAngles = v3;
        }
    }
}
