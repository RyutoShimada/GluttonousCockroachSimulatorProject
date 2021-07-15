using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FoodGenerater : MonoBehaviour
{
    [SerializeField] GameObject[] m_foods = null;
    [SerializeField] Transform[] m_generatePos = null;
    GameObject[] m_go;

    private void Start()
    {
        if (m_foods.Length <= 0) return;

        m_go = new GameObject[m_foods.Length];

        for (int i = 0; i < m_foods.Length; i++)
        {
            m_go[i] = Instantiate(m_foods[i], m_generatePos[i].position, m_generatePos[i].rotation, transform);
            m_go[i].SetActive(false);
        }
    }

    public void Generate(int generateCount)
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
        int[] randonPos = new int[generateCount];

        while (currentCount < generateCount)
        {
            randomFood[currentCount] = Random.Range(0, m_go.Length);
            randonPos[currentCount] = Random.Range(0, m_generatePos.Length);

            if (currentCount == 0)
            {
                m_go[randomFood[currentCount]].SetActive(true);
                m_go[randomFood[currentCount]].transform.position = m_generatePos[randonPos[currentCount]].position;
                currentCount++;
            }
            else
            {
                for (int i = currentCount; i > 0; i--)
                {
                    if (randomFood[currentCount] != randomFood[currentCount - i])
                    {
                        m_go[randomFood[currentCount]].SetActive(true);
                        m_go[randomFood[currentCount]].transform.position = m_generatePos[randonPos[currentCount]].position;
                        currentCount++;
                    }
                }
            }
        }
    }
}
