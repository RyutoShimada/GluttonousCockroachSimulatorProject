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
    int m_currentCount = 0;
    bool m_isCallChaild = false;
    bool m_isGenerating = false;
    Dictionary<int, GameObject> m_childs = new Dictionary<int, GameObject>();

    private void Awake()
    {
        Cockroach.Generate += Generate;
        //Human.CockChildAttack += DecreaseCount;
    }

    void Start()
    {
        Generate();
        UpdateText(m_currentCount);
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
            m_childs.Add(i, Instantiate(m_generatePrefab, transform));
            m_childs[i].GetComponent<NPCController>().Id = i;
            m_childs[i].SetActive(false);
        }
    }

    public void Generate(int count, Vector3 pos, Vector3 up)
    {
        int[] id = new int[count];
        int currentCount = 0;

        for (int i = 0; i < count; i++)
        {
            if (ActiveCheack(false) >= 0)
            {
                id[i] = ActiveCheack(false);
            }
            else
            {
                Debug.Log("MaxGenerate");
                break;
            }

            ActiveControll(id[i], true);
            int random = Random.Range(0, 360);
            m_childs[id[i]].gameObject.transform.position = pos;
            m_childs[id[i]].gameObject.transform.up = up;
            m_childs[id[i]].gameObject.transform.rotation = Quaternion.Euler(0, random, 0);
            Debug.Log(id[i]);
            currentCount++;
        }

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(ActiveRandomRotation), RpcTarget.Others, id, pos, up);
        }
        else
        {
            ActiveRandomRotation(id, pos, up);
        }

        m_currentCount += currentCount;
        UpdateText(m_currentCount);
    }

    [PunRPC]
    void ActiveRandomRotation(int[] id, Vector3 pos, Vector3 up)
    {
        int currentCount = 0;

        for (int i = 0; i < id.Length; i++)
        {
            ActiveControll(id[i], true);
            Debug.Log(id[i]);
            int random = Random.Range(0, 360);
            m_childs[id[i]].gameObject.transform.position = pos;
            m_childs[id[i]].gameObject.transform.up = up;
            m_childs[id[i]].gameObject.transform.rotation = Quaternion.Euler(0, random, 0);
            currentCount++;
        }

        m_currentCount += currentCount;
        UpdateText(m_currentCount);
    }

    public void DecreaseCount(int id)
    {
        if (PhotonNetwork.IsConnected)
        {
            // ニンゲン側の子ゴキが呼ぶ
            if (!m_isCallChaild) m_isCallChaild = true;
            photonView.RPC(nameof(DecreaseCountRPC), RpcTarget.All, id);
        }
        else
        {
            DecreaseCountRPC(id);
        }
    }

    [PunRPC]
    public void DecreaseCountRPC(int id)
    {
        m_currentCount--;
        UpdateText(m_currentCount);

        if (m_isCallChaild) return;
        Debug.Log("Decrease");
        ActiveControll(id, false);
    }

    void UpdateText(int count)
    {
        m_countText.text = count.ToString();

        if (m_currentCount >= m_generateCount)
        {
            NetWorkGameManager.Instance?.CockroachProliferationComplete();
        }
    }

    void ActiveControll(int id, bool active)
    {
        m_childs[id].SetActive(active);
    }

    /// <summary>
    /// アクティブかどうか確認して、IDを返す
    /// </summary>
    /// <param name="value">調べたい状態</param>
    /// <returns></returns>
    int ActiveCheack(bool value)
    {
        foreach (var item in m_childs)
        {
            if (value)
            {
                if (item.Value.activeSelf) return item.Key;
            }
            else
            {
                if (!item.Value.activeSelf) return item.Key;
            }
        }
        return -1;
    }

}
