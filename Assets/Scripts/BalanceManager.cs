using UnityEngine;

public class BalanceManager : MonoBehaviour
{
    private static BalanceManager instance;
    public static BalanceManager Instance => instance; // Read-only property
    private int balance;

    private void Awake()
    {
        if(instance == null)   
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            balance = PlayerPrefs.GetInt("Balance", 10000);
        } else {
            Destroy(gameObject);
        }
    }

    public int GetBalance()
    {
        return balance;
    }

    public void SetBalance(int newBalance)
    {
        balance = newBalance;
        PlayerPrefs.SetInt("Balance", balance);
    }
    
    public void AddToBalance(int amount)
    {
        balance += amount;
        PlayerPrefs.SetInt("Balance", balance);
    }
}
