using System.Collections;
using UnityEngine;
using Photon.Pun;

public class FoodGeneraterNetWork : MonoBehaviourPunCallbacks, IPunObservable, IFoodGenerate
{
    [SerializeField] GameObject[] m_foods = null;
    [SerializeField] Transform[] m_generatePos = null;
    [SerializeField] float m_interval = 2;
    [SerializeField] int m_generateCount = 2;
    GameObject[] m_go;
    Vector3 m_beforePos = Vector3.zero;

    private void Start()
    {
        if (m_foods.Length <= 0) return;

        m_go = new GameObject[m_foods.Length];

        for (int i = 0; i < m_foods.Length; i++)
        {
            m_go[i] = Instantiate(m_foods[i], m_generatePos[i].position, m_generatePos[i].rotation, transform);
            m_go[i].GetComponent<Food>().m_foodGeneraterNetWork = this;
            m_go[i].SetActive(false);
        }
    }

    public void Generate()
    {
        Debug.Log("Generate!");
        StartCoroutine(nameof(StartGenerate));
    }

    public IEnumerator StartGenerate()
    {
        if (!PhotonNetwork.IsMasterClient) yield return null;

        foreach (var item in m_go)
        {
            if (item.activeSelf)
            {
                item.SetActive(false);
            }
        }

        int currentCount = 0;
        int[] randomFood = new int[m_generateCount];
        int[] randomPos = new int[m_generateCount];

        yield return new WaitForSeconds(m_interval);

        while (currentCount < m_generateCount)
        {
            randomFood[currentCount] = Random.Range(0, m_go.Length);
            randomPos[currentCount] = Random.Range(0, m_generatePos.Length);

            // 前回と違う場所に生成するようにしている
            if (m_generatePos[randomPos[currentCount]].position == m_beforePos) continue;

            if (currentCount == 0)
            {
                ChangeFood(randomFood, randomPos, ref currentCount);
            }
            else
            {
                for (int i = currentCount; i > 0; i--)
                {
                    if (randomFood[currentCount] != randomFood[currentCount - i])
                    {
                        ChangeFood(randomFood, randomPos, ref currentCount);
                    }
                }
            }
        }

        Debug.Log("Generated!");
    }

    void ChangeFood(int[] randomFood, int[] randomPos, ref int currentCount)
    {
        m_go[randomFood[currentCount]].SetActive(true);
        m_go[randomFood[currentCount]].transform.position = m_generatePos[randomPos[currentCount]].position;
        m_beforePos = m_go[randomFood[currentCount]].transform.position;
        currentCount++;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < m_go.Length; i++)
            {
                stream.SendNext(m_go[i].transform.position);
                stream.SendNext(m_go[i].gameObject.activeSelf);
            }
        }
        else
        {
            for (int i = 0; i < m_go.Length; i++)
            {
                m_go[i].transform.position = (Vector3)stream.ReceiveNext();
                m_go[i].gameObject.SetActive((bool)stream.ReceiveNext());
            }
        }
    }
}