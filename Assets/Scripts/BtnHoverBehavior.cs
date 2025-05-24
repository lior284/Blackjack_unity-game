using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BtnHoverAction : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Text btnText;
    public Texture2D texture2D;
    private static bool allowColorChange = true;

    public static void SetColorChangeEnabled(bool state)
    {
        allowColorChange = state;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!allowColorChange && (btnText.text == "PLAY" || btnText.text == "Next Hand"))
        {
            return;
        }

        if(btnText.text != "Exit" && btnText.text != "Reset")
        {
            btnText.color = Color.white;
        } else {
            btnText.color = Color.red;
        }
        Cursor.SetCursor(texture2D, Vector2.zero, CursorMode.Auto); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(!allowColorChange && (btnText.text == "PLAY" || btnText.text == "Next Hand"))
        {
            return;
        }

        btnText.color = Color.black;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
