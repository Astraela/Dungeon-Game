using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject Main;
    public GameObject Saves;

    public void onStart(){
        Main.SetActive(false);
        Saves.SetActive(true);
    }

    public void Quit(){
        Application.Quit();
    }

    public void LoadSave(int save){
        //load save
        SceneManager.LoadScene(1);
    }

    public void Back(){
        Saves.SetActive(false);
        Main.SetActive(true);
    }
    
}
