using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//simply all the stats that need to be saved, except the multipliers
[System.Serializable]
public class Stats
{
    public int selectedSave = 0;
    [Header("Normal Stats")]
    public float Speed = 2f;
    public float Damage = 1;
    public float DashCD = 0.75f;
    public float SlamCD = 1.5f;
    public float Health = 100;
    public int Crystals = 0;

    [Header("UpgradeStats")]
    public float SpeedStep = .1f;
    public int SpeedCost = 10;
    public float SpeedMultiplier = .9f;
    public float DamageStep = .05f;
    public int DamageCost = 10;
    public float DamageMultiplier = .85f;
    public float DashCDStep = .01f;
    public int DashCDCost = 10;
    public float DashCDMultiplier = .7f;
    public float SlamCDStep = .05f;
    public int SlamCDCost = 10;
    public float SlamCDMultiplier = .6f;

    public float CostMultiplier = 1.5f;
}