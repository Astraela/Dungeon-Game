using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    public float Speed = 2f;
    public float Damage = 1;
    public float DashCD = 0.75f;
    public float SlamCD = 1.5f;

    float _Health = 100;
    public float Health { 
        get{return _Health;}
        set{_Health = value;
            GetComponent<GameUI>().SetHealth(value);
            }
    }

    int _Crystals = 0;
    public int Crystals { 
        get{return _Crystals;}
        set{_Crystals = value;
            GetComponent<GameUI>().SetCrystals(value);
            }
    }

}