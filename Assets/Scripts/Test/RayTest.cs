using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class RayTest : MonoBehaviour
{
    [SerializeField] Transform m_rayDir = null;
    [SerializeField] Transform m_rayOriginPos = null;
    float m_distance;
    float m_saveDistance;
    RaycastHit m_hit;

    [SerializeField] int speed = 5;
    float x;
    float y;

    Vector3 m_nomal = Vector3.zero;

    bool m_isRotate = false;

    void Start()
    {
        m_distance = Vector3.Distance(transform.position, m_rayDir.position);
        m_saveDistance = m_distance;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_distance > m_saveDistance)
        {
            m_distance = m_saveDistance;
        }

        if (Physics.Raycast(transform.position, m_rayDir.position - transform.position, out m_hit, m_distance))
        {
            if (m_hit.normal == m_nomal)
            {
                m_rayDir.position = m_rayOriginPos.position;
                return;
            }
            else
            {
                Debug.Log(m_hit.collider.gameObject.name);
                m_nomal = m_hit.normal;
                m_isRotate = true;
                Quaternion toRotate = Quaternion.FromToRotation(transform.up, m_nomal) * transform.rotation;
                transform.DORotateQuaternion(toRotate, 3f).OnComplete(() => { m_isRotate = false; });
            }
        }
        else
        {
            if (m_isRotate) return;

            x = m_distance * Mathf.Sin(Time.time * speed);
            y = m_distance * Mathf.Cos(Time.time * speed);

            if (y + transform.localPosition.y > transform.localPosition.y)
            {
                y = 0;
                x = m_saveDistance;
            }

            m_rayDir.position = new Vector3(x + transform.localPosition.x, y + transform.localPosition.y, transform.localPosition.z);
        }

        Debug.DrawRay(transform.position, m_rayDir.position - transform.position, Color.red);
    }

    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnCollisionExit(Collision collision)
    {

    }
}
