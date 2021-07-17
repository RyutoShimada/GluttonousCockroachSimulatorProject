using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FoodGenerater : MonoBehaviour
{
    [SerializeField] GameObject[] m_foods = null;
    [SerializeField] Transform[] m_generatePos = null;
    GameObject[] m_go;
    Vector3 m_beforePos = Vector3.zero;

    private void Awake()
    {
        m_go = new GameObject[m_foods.Length];

        for (int i = 0; i < m_foods.Length; i++)
        {
            m_go[i] = Instantiate(m_foods[i], m_generatePos[i].position, m_generatePos[i].rotation, transform);
            m_go[i].SetActive(false);
        }
    }

    private void Start()
    {
        if (m_foods.Length <= 0) return;
    }

    public IEnumerator Generate(int generateCount, float interval)
    {
        foreach (var item in m_go)
        {
            if (item.activeSelf)
            {
                item.SetActive(false);
            }
        }

        int currentCount = 0;
        int[] randomFood = new int[generateCount];
        int[] randomPos = new int[generateCount];

        yield return new WaitForSeconds(interval);

        while (currentCount < generateCount)
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
    }

    void ChangeFood(int[] randomFood, int[] randomPos, ref int currentCount)
    {
        m_go[randomFood[currentCount]].SetActive(true);
        m_go[randomFood[currentCount]].transform.position = m_generatePos[randomPos[currentCount]].position;
        m_beforePos = m_go[randomFood[currentCount]].transform.position;
        currentCount++;
        Debug.Log("Generated!");
    }
}
