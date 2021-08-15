using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineOfTheSelectedChracter : MonoBehaviour
{
    [SerializeField] CockroachOutline m_cockroachOutline = null;
    [SerializeField] HumanOutline m_humanOutline = null;

    Vector3 m_cursorPosition;
    Vector3 m_cursorPosition3d;
    RaycastHit m_hit;

    // Update is called once per frame
    void Update()
    {
        OnRay();
    }

    void OnRay()
    {
        m_cursorPosition = Input.mousePosition; // 画面上のカーソルの位置
        m_cursorPosition.z = 10.0f; // z座標に適当な値を入れる
        m_cursorPosition3d = Camera.main.ScreenToWorldPoint(m_cursorPosition); // 3Dの座標になおす

        // カメラから cursorPosition3d の方向へレイを飛ばす
        if (Physics.Raycast(Camera.main.transform.position, (m_cursorPosition3d - Camera.main.transform.position), out m_hit, Mathf.Infinity))
        {
            Debug.DrawRay(Camera.main.transform.position, (m_cursorPosition3d - Camera.main.transform.position) * m_hit.distance, Color.red);

            if (m_hit.collider.gameObject.tag == "Cockroach")
            {
                if (!m_cockroachOutline.enabled)
                {
                    m_cockroachOutline.enabled = true;
                }
            }
            else if (m_hit.collider.gameObject.tag == "Human")
            {
                if (!m_humanOutline.enabled)
                {
                    m_humanOutline.enabled = true;
                }
            }
            else
            {
                UnEnabled();
            }
        }
        else
        {
            UnEnabled();
        }
    }

    void UnEnabled()
    {
        if (m_cockroachOutline.enabled)
        {
            m_cockroachOutline.enabled = false;
        }

        if (m_humanOutline.enabled)
        {
            m_humanOutline.enabled = false;
        }
    }
}
