using UnityEngine;

/// <summary>
/// 食べ物にアタッチし、オブジェクトプールで生成する。
/// </summary>
public class Food : MonoBehaviour
{
    /// <summary>FoodGeneraterNetWork からセットする</summary>
    [HideInInspector] public FoodGenerater m_foodGeneraterNetWork = null;

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
