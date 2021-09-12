using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelecteController : MonoBehaviour
{
    [SerializeField] CockroachOutline m_cockroachOutline = null;
    [SerializeField] HumanOutline m_humanOutline = null;

    public void OnCockroachSelect()
    {
        if (!m_cockroachOutline.enabled)
        {
            m_cockroachOutline.enabled = true;
        }
    }

    public void OnHumanSelect()
    {
        if (!m_humanOutline.enabled)
        {
            m_humanOutline.enabled = true;
        }
    }

    public void UnEnabled()
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
