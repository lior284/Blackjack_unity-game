using UnityEngine;

public class Cardback : MonoBehaviour
{
    private static Cardback instance;
    public static Cardback Instance => instance;

    private Sprite selectedCardBack;
    private readonly string[] colors = {"red", "blue", "green", "orange", "purple", "black"};

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            selectedCardBack = Resources.Load<Sprite>("Cards/Backs/card_back_" + colors[PlayerPrefs.GetInt("CardBackIndex")]);
        } else {
            Destroy(gameObject);
        }
    }

    public void SetSelectedCardBack(int index)
    {
        selectedCardBack = Resources.Load<Sprite>("Cards/Backs/card_back_" + colors[index]);
        PlayerPrefs.SetInt("CardBackIndex", index);
    }

    public Sprite GetSelectedCardBack()
    {
        return selectedCardBack;
    }
    
    public int GetSelectedCardBackIndex()
    {
        return PlayerPrefs.GetInt("CardBackIndex");
    }
}
