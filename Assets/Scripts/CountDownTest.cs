using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownTest : MonoBehaviour
{
    [SerializeField] GameObject[] m_countDown = null;

    void Start()
    {
        StartCoroutine(CoroutineGameStart(3));
    }

    IEnumerator CoroutineGameStart(int waitSeconds)
    {
        for (int i = waitSeconds; i >= 0; i--)
        {
            yield return new WaitForSeconds(1f);

            if (i != 0)
            {
                if (i < m_countDown.Length)
                {
                    m_countDown[i].SetActive(false);
                }

                m_countDown[i - 1].SetActive(true);
            }
            else
            {
                m_countDown[0].SetActive(false);
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
