using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamStopEffect : MonoBehaviour
{
    [SerializeField] GameObject m_stopEffect = null;
    [SerializeField] LayerMask m_layerMask;
    RaycastHit m_hit;
    float m_distance;

    private void Start()
    {
        m_distance = GetComponent<BoxCollider>().size.z;
    }

    // Update is called once per frame
    void Update()
    {
        if (Physics.Raycast(transform.position, transform.forward, out m_hit, m_distance, m_layerMask))
        {
            if (!m_stopEffect.activeSelf) m_stopEffect.SetActive(true);
            m_stopEffect.transform.position = m_hit.point;
        }
    }
}
