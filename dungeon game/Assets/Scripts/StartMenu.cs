using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public GameObject Main;
    public GameObject Saves;
    public GameObject Game;

    public void Start(){
        Game = GameObject.FindObjectOfType<Game>().gameObject;
    }

    public void onStart(){
        Main.SetActive(false);
        Saves.SetActive(true);
    }

    public void Quit(){
        Application.Quit();
    }

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
        DontDestroyOnLoad(Game);
        Game.GetComponent<Game>().stats.selectedSave = save;
        SceneManager.LoadScene(1);
    }

    public void Back(){
        Saves.SetActive(false);
        Main.SetActive(true);
    }
    
}
