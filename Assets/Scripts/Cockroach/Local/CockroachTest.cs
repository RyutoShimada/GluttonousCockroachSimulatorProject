using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゴキブリのスクリプト
/// </summary>
public class CockroachTest : MonoBehaviour
{
    /// <summary>最大の体力値</summary>
    [SerializeField] int m_maxHp = 100;
    /// <summary>現在の体力値</summary>
    [SerializeField] int m_hp = 100;
    /// <summary>無敵モード時間</summary>
    [SerializeField] float m_invincibleModeTime = 3f;
    /// <summary>加算する速度</summary>
    [SerializeField] float m_addSpeedValue = 5f;
    /// <summary>加算するジャンプ力</summary>
    [SerializeField] float m_addJumpValue = 5f;
    /// <summary>無敵モード</summary>
    [SerializeField] bool m_invincibleMode = false;

    [SerializeField] GameObject m_generatePrefab = null;
    [SerializeField] int m_generateCount = 10;

    CockroachMoveControllerTest m_CMC;
    CockroachUITest m_CU;
    GameManager m_GM;

    /// <summary>死んだかどうか</summary>
    public bool m_isDed = false;

    private void Start()
    {
        m_isDed = false;
        m_hp = m_maxHp;
        m_CMC = GetComponent<CockroachMoveControllerTest>();
        m_CU = GetComponent<CockroachUITest>();
        m_CU?.ReflectHPSlider(m_hp, m_maxHp);
        m_GM = FindObjectOfType<GameManager>();
    }

    private void Update()
    {
        if (m_isDed) return;
    }

    void CheckAlive()
    {
        if (m_hp > 0) return;
        m_hp = 0;
        m_isDed = true;
        m_CMC.IsDed = true;
        Debug.Log("Ded!");
    }

    /// <summary>
    /// 無敵モード
    /// </summary>
    IEnumerator InvincibleMode()
    {
        Debug.Log("無敵モード開始");
        // 無敵モード開始
        m_invincibleMode = true;
        m_CMC.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);

        yield return new WaitForSeconds(m_invincibleModeTime);

        Debug.Log("無敵モード停止");
        // 無敵モード停止
        m_invincibleMode = false;
        m_CMC.InvincibleMode(m_invincibleMode, m_addSpeedValue, m_addJumpValue);
    }

    /// <summary>
    /// 食べ物を食べて、満腹ゲージを回復する。
    /// </summary>
    /// <param name="heelValue">体力に加算する値</param>
    void Eat()
    {
        if (m_generatePrefab)
        {
            Generate(m_generatePrefab, m_generateCount);
        }
        
        m_GM?.FoodGenerate();
        Debug.Log("Eat");
    }

    void Generate(GameObject obj, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Instantiate(obj, transform.position, Quaternion.identity);
        }
    }

    /// <summary>
    /// 攻撃されたときに呼ばれる。体力を減らす。 
    /// </summary>
    /// <param name="damageValue">体力を減算する値</param>
    public void BeAttacked(int damageValue)
    {
        if (m_isDed) return;
        if (m_invincibleMode) return;

        m_hp -= damageValue;
        CheckAlive();                           // 生存確認
        StartCoroutine(InvincibleMode());       // 無敵モード開始
        StartCoroutine(m_CU.DamageColor());     // ダメージを受けたUI表示
        m_CU.ReflectHPSlider(m_hp, m_maxHp);    // HPバーを減少させる
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Food")
        {
            Eat();
            other.GetComponent<Food>()?.UnActive();
        }
    }
}
