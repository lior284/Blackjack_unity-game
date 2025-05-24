using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    public string playerName;
    private string playerPassword;
    public double playerBalance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlayer(string name, string password)
    {
        playerName = name;
        playerPassword = password;
        playerBalance = 10000;
    }
}
