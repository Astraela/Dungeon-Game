using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script voor het Escape menu
public class EscMenu : MonoBehaviour
{
    Canvas canvas;
    
    void Start(){
        canvas = GetComponentInChildren<Canvas>();
    }

    //Wanneer de player op ESC drukt, menu aan of uit schakelen
    public void OnEsc(){
        if(canvas.enabled){
            Time.timeScale = 1;
            canvas.enabled = false;
            return;
        }
        Time.timeScale = 0;
        canvas.enabled = true;
    }

    //de "continue" knop
    public void OnContinue(){
        Time.timeScale = 1;
        canvas.enabled = false;
    }

    //de "quit to menu" knop
    public void OnMenu(){
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
