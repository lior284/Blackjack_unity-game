using UnityEngine;
using UnityEngine.UI;

public class ShowBalance : MonoBehaviour
{
    public Text balanceText;

    public void Start()
    {
        balanceText.text = "Balance: " + FullBalance(BalanceManager.Instance.GetBalance());
    }

    public static string FullBalance(int balance) // 10000 -> 10,000$ | 1234567 -> 1,234,567$
    {
        string fullBalance = "";
        for(int i=balance.ToString().Length-1; i>=0; i--)
        {
            fullBalance += balance.ToString()[balance.ToString().Length - 1 - i];
            if(i != 0 && i % 3 == 0)
            {
                fullBalance += ",";
            }
        }
        return fullBalance + "$";
    }
}
