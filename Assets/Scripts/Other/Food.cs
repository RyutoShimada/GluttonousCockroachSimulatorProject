using UnityEngine;

/// <summary>
/// 食べ物にアタッチし、オブジェクトプールで生成する。
/// </summary>
public class Food : MonoBehaviour
{
    /// <summary>回復する値</summary>
    public int m_heelValue = 10;
    /// <summary>FoodGeneraterNetWork からセットする</summary>
    public FoodGeneraterNetWork m_foodGeneraterNetWork = null;

    /// <summary>
    /// このオブジェクトを非アクティブにする。(Cockroachから呼ばれる)
    /// </summary>
    public void UnActive()
    {
        if (m_foodGeneraterNetWork)
        {
            m_foodGeneraterNetWork.GetComponent<IFoodGenerate>().Generate();
        }

        this.gameObject.SetActive(false);
    }
}
