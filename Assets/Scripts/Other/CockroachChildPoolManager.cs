using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CockroachChildPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject m_generatePrefab = null;
    [SerializeField] int m_generateCount = 100;
    Quaternion[] m_randomDirection;

    void Start()
    {
        Generate();
        SubscribeDirection();
        ActiveRandomRotation(m_generateCount, transform.position + Vector3.up);
    }

    void SubscribeDirection()
    {
        m_randomDirection = new Quaternion[8];
        for (int i = 0; i < m_randomDirection.Length; i++)
        {
            // 45度ずつ登録
            m_randomDirection[i] = new Quaternion(0, i * 45, 0, 0);
        }
    }

    void Generate()
    {
        for (int i = 0; i < m_generateCount; i++)
        {
            GameObject go = Instantiate(m_generatePrefab, transform);
            go.SetActive(false);
        }
    }

    public void ActiveRandomRotation(int count, Vector3 pos)
    {
        int i = 0;

        foreach (Transform t in transform)
        {
            if (i == count) return;
            if (!t.gameObject.activeSelf)
            {
                int random = Random.Range(0, m_randomDirection.Length);
                t.gameObject.SetActive(true);
                t.gameObject.transform.position = pos;
                t.gameObject.transform.rotation = m_randomDirection[random];
                i++;
            }
        }
    }
}
