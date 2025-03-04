using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class InputFieldChecker : MonoBehaviour
{
    public TMP_InputField inputField; // —сылка на TMP_InputField
    public  GameObject Fadein1; // —сылка на TextMeshProUGUI дл€ отображени€ результата
    public  GameObject Fadein2; // —сылка на TextMeshProUGUI дл€ отображени€ результата
    public  GameObject Fadein3; // —сылка на TextMeshProUGUI дл€ отображени€ результата
    private bool _check = false;
    private void Update()
    {
        CheckInput();
    }

    // ћетод дл€ проверки значени€
    public void CheckInput()
    {
        if (_check == false & inputField.text == "start")
        {
            Debug.Log("spam");
            Fadein1.SetActive(true);
            _check = true;
        }
        if (_check == false & inputField.text == "Level2")
        {
            Debug.Log("spam");
            Fadein2.SetActive(true);
            _check = true;
        }
        if (_check == false & inputField.text == "Level3")
        {
            Debug.Log("spam");
            Fadein3.SetActive(true);
            _check = true;
        }
    }
}
