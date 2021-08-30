using UnityEngine;
using DG.Tweening;

public class RayTest : MonoBehaviour
{
    [SerializeField] Transform m_rayStartPos = null;
    [SerializeField] Transform m_rayEndPos = null;
    [SerializeField] Transform m_rayDirPos = null;
    [SerializeField] float m_speed = 1f;
    [SerializeField] float m_stopingDistance = 0;
    float m_distance = 0;
    RaycastHit m_hit;
    Vector3 m_nomal = Vector3.zero;
    bool m_isRotate = false;
    /// <summary>rayStartPosからrayEndPosへの方向</summary>
    Vector3 m_dir;

    void Start()
    {
        m_distance = Vector3.Distance(transform.position, m_rayStartPos.position);
        m_rayDirPos.position = m_rayStartPos.position;
        m_dir = m_rayStartPos.position - m_rayEndPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, m_rayDirPos.position - transform.position, out m_hit, m_distance))
        {
            m_rayDirPos.position = m_rayStartPos.position;

            if (m_hit.normal == m_nomal)
            {
                return;
            }
            else
            {
                Debug.Log(m_hit.collider.gameObject.name);
                m_nomal = m_hit.normal;
                m_isRotate = true;
                Quaternion toRotate = Quaternion.FromToRotation(transform.up, m_nomal) * transform.rotation;
                transform.DORotateQuaternion(toRotate, 3f).OnComplete(() =>
                {
                    m_isRotate = false;
                    m_dir = m_rayStartPos.position - m_rayEndPos.position;
                });
            }
        }
        else
        {
            if (m_isRotate) return;
            m_rayDirPos.position -= m_dir * (Time.deltaTime * m_speed);
            float dis = Vector3.Distance(m_rayDirPos.position, m_rayEndPos.position);
            if (dis < m_stopingDistance || dis > m_distance)
            {
                m_rayDirPos.position = m_rayStartPos.position;
            }
        }

        Debug.DrawRay(transform.position, (m_rayDirPos.position - transform.position).normalized * m_distance, Color.red);
    }
}
