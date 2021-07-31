﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float m_sensitivity = 1f; // いわゆるマウス感度
    [SerializeField] float m_mouseYMaxRange = 300f;
    [SerializeField] float m_mouseYMinRange = 0f;

    bool m_canMove = true;

    private void Start()
    {
        EventSystem.Instance.Subscribe((EventSystem.CanMove)CanMove);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_canMove)
        {
            CameraControlle();
        }
    }

    /// <summary>
    /// 動けるかどうか（イベントから呼ばれる）
    /// </summary>
    /// <returns></returns>
    void CanMove(bool isMove)
    {
        m_canMove = isMove;
    }

    void CameraControlle()
    {
        float mouse_move_y = Input.GetAxis("Mouse Y") * m_sensitivity;

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