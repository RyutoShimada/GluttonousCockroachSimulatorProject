using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

public class CockroachChildPoolManager : MonoBehaviourPunCallbacks
{
    
    [SerializeField] GameObject m_generatePrefab = null;
    [SerializeField] int m_generateCount = 100;
    [SerializeField] Text m_countText = null;
    int m_currentCount = 0;

    void Start()
    {
        Generate();
        Cockroach.GenerateChild += Generate;
    }

    void Generate()
    {
        for (int i = 0; i < m_generateCount; i++)
        {
            GameObject go = Instantiate(m_generatePrefab, transform);
            go.SetActive(false);
        }
    }

    void Generate(int count, Vector3 pos, Vector3 up)
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(ActiveRandomRotation), RpcTarget.All, count, pos, up);
        }
        else
        {
            ActiveRandomRotation(count, pos, up);
        }
    }

    [PunRPC]
    void ActiveRandomRotation(int count, Vector3 pos, Vector3 up)
    {
        int i = 0;
        foreach (Transform t in transform)
        {
            if (i == count) break;
            if (!t.gameObject.activeSelf)
            {
                int random = Random.Range(0, 360);
                t.gameObject.SetActive(true);
                t.gameObject.transform.position = pos;
                t.gameObject.transform.up = up;
                t.gameObject.transform.rotation = Quaternion.Euler(0, random, 0);
                i++;
            }
        }
        UpdateText(m_currentCount + i);
    }

    public void DecreaseCount()
    {
        UpdateText(m_currentCount - 1);
    }

    void UpdateText(int count)
    {
        // 数値を滑らかに変動させている
        DOTween.To(() => m_currentCount, n => m_currentCount = n, count, 0.5f)
            .OnUpdate(() => m_countText.text = m_currentCount.ToString());
    }
}
