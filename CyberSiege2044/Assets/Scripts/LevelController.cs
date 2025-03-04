using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelcontroler : MonoBehaviour
{
    public int scene;
    public void changescene()
    {
        SceneManager.LoadScene(scene);
    }
}