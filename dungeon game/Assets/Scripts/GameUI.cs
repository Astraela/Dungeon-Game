using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public Slider health;
    public Text crystals;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetHealth(float h){
        health.value = h/100;
    }

    public void SetCrystals(int c){
        crystals.text = c.ToString();
    }
}
