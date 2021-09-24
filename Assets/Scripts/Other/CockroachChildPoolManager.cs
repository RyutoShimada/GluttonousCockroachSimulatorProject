using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class CockroachChildPoolManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject m_generatePrefab = null;
    [SerializeField] int m_generateCount = 100;
    [SerializeField] Text m_countText = null;
    [SerializeField] bool m_isTestPlay = false;
    int m_currentCount = 0;
    bool m_isCallChaild = false;
    bool m_isGenerating = false;

    private void Awake()
    {
        Cockroach.Generate += Generate;
        //Human.CockChildAttack += DecreaseCount;
    }

    void Start()
    {   
        Generate();
        UpdateText(m_currentCount);
        if (m_isTestPlay)
        {
            ActiveRandomRotation(100, Vector3.zero, Vector3.up);
        }
    }

    private void OnDestroy()
    {
        Cockroach.Generate -= Generate;
        //Human.CockChildAttack -= DecreaseCount;
    }

    void Generate()
    {
        for (int i = 0; i < m_generateCount; i++)
        {
            GameObject go = Instantiate(m_generatePrefab, transform);
            go.SetActive(false);
        }
    }

    public void Generate(int count, Vector3 pos, Vector3 up)
    {
        if (m_isGenerating) return;
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
        m_isGenerating = true;
        int currentCount = 0;
        foreach (Transform child in gameObject.transform)
        {
            if (currentCount == count) break;
            if (!child.gameObject.activeSelf)
            {
                int random = Random.Range(0, 360);
                child.gameObject.SetActive(true);
                child.gameObject.transform.position = pos;
                child.gameObject.transform.up = up;
                child.gameObject.transform.rotation = Quaternion.Euler(0, random, 0);
                currentCount++;
            }
        }

        m_currentCount += currentCount;
        UpdateText(m_currentCount);
        m_isGenerating = false;
    }

    public void DecreaseCount()
    {
        if (PhotonNetwork.IsConnected)
        {
            // ニンゲン側の子ゴキが呼ぶ
            if (!m_isCallChaild) m_isCallChaild = true;
            photonView.RPC(nameof(DecreaseCountRPC), RpcTarget.All);
        }
        else
        {
            DecreaseCountRPC();
        }
    }

    [PunRPC]
    public void DecreaseCountRPC()
    {
        m_currentCount--;
        UpdateText(m_currentCount);
        Debug.Log($"Decrease : currentCount {m_currentCount}");

        if (m_isCallChaild) return;

        foreach (Transform child in gameObject.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                break;
            }
        }
    }

    void UpdateText(int count)
    {
        m_countText.text = count.ToString();

        if (m_currentCount >= m_generateCount)
        {
            NetWorkGameManager.Instance?.CockroachProliferationComplete();
        }
    }
}
