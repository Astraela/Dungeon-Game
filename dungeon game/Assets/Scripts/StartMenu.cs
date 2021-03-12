using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

//script for the sart menu
public class StartMenu : MonoBehaviour
{
    public GameObject Main;
    public GameObject Saves;
    public GameObject Game;

    public void Start(){
        Game = GameObject.FindObjectOfType<Game>().gameObject;
    }

    //switch to saves screen
    public void onStart(){
        Main.SetActive(false);
        Saves.SetActive(true);
    }

    //quit game
    public void Quit(){
        Application.Quit();
    }

    //load the save
    public void LoadSave(int save){
        string fullPath = "";
        if(Application.isEditor){
            fullPath = Application.dataPath + "/Save" + save + ".txt";
        }
        else
        {
            fullPath = Application.persistentDataPath + "/Save" + save + ".txt";
        }

        if(File.Exists(fullPath)){
            StreamReader reader = new StreamReader(fullPath);
            //Debug.Log(reader.ReadToEnd());
            Stats saveSettings = JsonUtility.FromJson<Stats>(reader.ReadToEnd());
            Stats stats = Game.GetComponent<Game>().stats;
            print(saveSettings);
            stats.Speed = saveSettings.Speed;
            stats.Damage = saveSettings.Damage;
            stats.DashCD = saveSettings.DashCD;
            stats.SlamCD = saveSettings.SlamCD;
            stats.Crystals = saveSettings.Crystals;
            stats.SpeedStep = saveSettings.SpeedStep;
            stats.SpeedCost = saveSettings.SpeedCost;
            stats.DamageStep = saveSettings.DamageStep;
            stats.DamageCost = saveSettings.DamageCost;
            stats.DashCDStep = saveSettings.DashCDStep;
            stats.DashCDCost = saveSettings.DashCDCost;
            stats.SlamCDStep = saveSettings.SlamCDStep;
            stats.SlamCDCost = saveSettings.SlamCDCost;
            reader.Close();
            reader.Dispose();
        }
        //set game object on dontdestroy so stats stay over scenes, set the selectedsave as the correct save slot
        //loads scene
        DontDestroyOnLoad(Game);
        Game.GetComponent<Game>().stats.selectedSave = save;
        SceneManager.LoadScene(1);
    }

    // go back to main menu
    public void Back(){
        Saves.SetActive(false);
        Main.SetActive(true);
    }
    
}
