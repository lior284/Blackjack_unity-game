using System.Data;
using Mono.Data.Sqlite;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Register : MonoBehaviour
{
    public GameObject nameInput;
    public GameObject passwordInput;
    public GameObject passwordAgainInput;

    private Text nameInputText;
    private Text passwordInputText;
    private Text passwordAgainInputText;

    private void Start()
    {
        nameInputText = nameInput.GetComponentsInChildren<GameObject>()[1].GetComponent<Text>();
        passwordInputText = passwordInput.GetComponentsInChildren<GameObject>()[1].GetComponent<Text>();
        passwordAgainInputText = passwordAgainInput.GetComponentsInChildren<GameObject>()[1].GetComponent<Text>();
    }

    public void LeaveNameInput()
    {
        if(nameInputText.text.Trim() == "")
        {
            nameInputText.color = new Color (1, 0, 0);
        }
    }

    public void LeavePasswordInput()
    {
        if(passwordInputText.text.Trim() == "" || passwordInputText.text.Length < 4 || passwordInputText.text.Contains(" "))
        {
            passwordInputText.color = new Color (1, 0, 0);
        }

        SqliteConnection connection = new SqliteConnection("URI=file:" + Application.persistentDataPath + "/gameDatabase.db");
        connection.Open();
        SqliteCommand command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM players WHERE password='" + passwordInputText.text + "'";
        IDataReader reader = command.ExecuteReader();
        if(!reader.Read())
        {
            passwordInputText.color = new Color (1, 0, 0);
        }
        connection.Close();
    }
    
    public void LeavePasswordAgainInput()
    {
        if(passwordAgainInputText.text.Trim() == "" || passwordAgainInputText.text.Length < 4 || passwordAgainInputText.text.Contains(" ") || passwordAgainInputText.text != passwordInputText.text)
        {
            passwordAgainInputText.color = new Color (1, 0, 0);
        }
    }

    public void RegisterBtnClick()
    {
        bool allValid = true;

        if(nameInputText == null || nameInputText.text.Trim() == "")
        {
            nameInputText.color = new Color (1, 0, 0);
            allValid = false;
        }

        if(passwordInputText == null || passwordInputText.text.Trim() == "" || passwordInputText.text.Length < 4 || passwordInputText.text.Contains(" "))
        {
            passwordInputText.color = new Color (1, 0, 0); 
            allValid = false;
        }

        if(passwordAgainInputText == null || passwordAgainInputText.text.Trim() == "" || passwordAgainInputText.text.Length < 4 || 
            passwordAgainInputText.text.Contains(" ") || passwordAgainInputText.text != passwordInputText.text)
        {
            passwordAgainInputText.color = new Color (1, 0, 0);
            allValid = false;
        }

        if(allValid)
        {
            SqliteConnection connection = new SqliteConnection("URI=file:" + Application.persistentDataPath + "/gameDatabase.db");
            connection.Open();
            SqliteCommand command = connection.CreateCommand();

            command.CommandText = "INSERT INTO players (name, password, balance) VALUES (" + nameInputText.text + ", " + passwordInputText.text + ", " + 10000 + ")";
            command.ExecuteNonQuery();
            connection.Close();

            PlayerData.instance.RegisterPlayer(nameInputText.text, passwordInputText.text);

            SceneManager.LoadScene("Start");
        }
    }
}
