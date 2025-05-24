using UnityEngine;

public class ResetChips : MonoBehaviour
{
    public void ResetPlayerChips()
    {
        BalanceManager.Instance.SetBalance(10000);
    }
}
