using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CockroachChildPoolManager : MonoBehaviour
{
    [SerializeField] GameObject m_generatePrefab = null;
    [SerializeField] int m_generateCount = 100;
    [SerializeField] Text m_countText = null;
    [SerializeField] bool m_isTestPlay = false;
    int m_currentCount = 0;

    private void Awake()
    {
        Cockroach.Generate += Generate;
        Human.CockChildAttack += DecreaseCount;
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
        Human.CockChildAttack -= DecreaseCount;
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
        ActiveRandomRotation(count, pos, up);
    }

    void ActiveRandomRotation(int count, Vector3 pos, Vector3 up)
    {
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
    }

    public void DecreaseCount(bool isCallChaild)
    {
        m_currentCount--;
        UpdateText(m_currentCount);
        Debug.Log($"Decrease : currentCount {m_currentCount}");

        if (isCallChaild) return;

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
