using UnityEngine;

public class ClearPlayerPrefsButton : MonoBehaviour
{
    public void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs очищены!");
    }
}