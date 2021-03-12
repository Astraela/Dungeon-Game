using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

//script for the upgrade ui on the upgrade menu
public class UpgradeUI : MonoBehaviour
{
    public Text speedCur;
    public Text damageCur;
    public Text dashCur;
    public Text slamCur;

    public Text speedCost;
    public Text damageCost;
    public Text dashCost;
    public Text slamCost;

    public Text crystals;

    string curText = "Currently: ";
    string costText = "Upgrade for ";
    string costText2 = " Crystals";

    //returns the number with 3 decimals max
    string Floor(float nr){
        return (Mathf.Floor(nr*1000)/1000).ToString();
    }

    //setup the values based on stats
    void SetupValues(){
        var stats = GameObject.FindObjectOfType<Game>().stats;
        speedCur.text = curText + Floor(stats.Speed);
        damageCur.text = curText + Floor(stats.Damage);
        dashCur.text = curText + Floor(stats.DashCD);
        slamCur.text = curText + Floor(stats.SlamCD);

        speedCost.text = costText + stats.SpeedCost + costText2;
        damageCost.text = costText + stats.DamageCost + costText2;
        dashCost.text = costText + stats.DashCDCost + costText2;
        slamCost.text = costText + stats.SlamCDCost + costText2;

        crystals.text = stats.Crystals.ToString();
    }

    //wait 1 frame so the unneeded Game object can delete itself
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        SetupValues();
    }

    //if speed is upgraded
    public void SpeedUpgrade(){
        var stats = GameObject.FindObjectOfType<Game>().stats;
        if(stats.Crystals < stats.SpeedCost){
            return;
        }
        stats.Crystals -= stats.SpeedCost;
        stats.Speed += stats.SpeedStep;
        stats.SpeedStep *= stats.SpeedMultiplier;
        stats.SpeedCost = (int)(stats.SpeedCost * stats.CostMultiplier);

        speedCur.text = curText + Floor(stats.Speed);
        speedCost.text = costText + stats.SpeedCost + costText2;
        crystals.text = stats.Crystals.ToString();

    }

    //if Damage is upgraded
    public void DamageUpgrade(){
        var stats = GameObject.FindObjectOfType<Game>().stats;
        if(stats.Crystals < stats.DamageCost){
            return;
        }
        stats.Crystals -= stats.DamageCost;
        stats.Damage+= stats.DamageStep;
        stats.DamageStep *= stats.DamageMultiplier;
        stats.DamageCost = (int)(stats.DamageCost * stats.CostMultiplier);

        damageCur.text = curText + Floor(stats.Damage);
        damageCost.text = costText + stats.DamageCost + costText2;
        crystals.text = stats.Crystals.ToString();
    }

    //if Dash is upgraded
    public void DashUpgrade(){
        var stats = GameObject.FindObjectOfType<Game>().stats;
        if(stats.Crystals < stats.DashCDCost){
            return;
        }
        stats.Crystals -= stats.DashCDCost;
        stats.DashCD -= stats.DashCDStep;
        stats.DashCDStep *= stats.DashCDMultiplier;
        stats.DashCDCost = (int)(stats.DashCDCost * stats.CostMultiplier);

        dashCur.text = curText + Floor(stats.DashCD);
        dashCost.text = costText + stats.DashCDCost + costText2;
        crystals.text = stats.Crystals.ToString();
    }

    //if slam is upgraded
    public void SlamUpgrade(){
        var stats = GameObject.FindObjectOfType<Game>().stats;
        if(stats.Crystals < stats.SlamCDCost){
            return;
        }
        stats.Crystals -= stats.SlamCDCost;
        stats.SlamCD -= stats.SlamCDStep;
        stats.SlamCDStep *= stats.SlamCDMultiplier;
        stats.SlamCDCost = (int)(stats.SlamCDCost * stats.CostMultiplier);

        slamCur.text = curText + Floor(stats.SlamCD);
        slamCost.text = costText + stats.SlamCDCost + costText2;
        crystals.text = stats.Crystals.ToString();
    }

    //if back/play is pressed, to save
    void Save(){
        var stats = GameObject.FindObjectOfType<Game>().stats;
        string fullPath = "";
        if(Application.isEditor){
            fullPath = Application.dataPath + "/Save" + stats.selectedSave + ".txt";
        }
        else
        {
            fullPath = Application.persistentDataPath + "/Save" + stats.selectedSave + ".txt";
        }
        string content = JsonUtility.ToJson(stats);
        FileStream file = File.Open(fullPath,FileMode.Create);
        StreamWriter writer = new StreamWriter(file);
        writer.Write(content);
        writer.Close();
        file.Close();
    }

    //on play
    public void Play(){
        Save();
        var stats = GameObject.FindObjectOfType<Game>().stats;
        GameObject.FindObjectOfType<Game>().SetCrystals(stats.Crystals);
        GameObject.FindObjectOfType<Game>().canvas.SetActive(true);
        SceneManager.LoadScene(2);
    }
    //on back
    public void Back(){
        Save();
        SceneManager.LoadScene(0);
        Destroy(GameObject.FindObjectOfType<Game>().gameObject);
    }
}
