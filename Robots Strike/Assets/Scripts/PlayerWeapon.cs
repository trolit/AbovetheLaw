﻿using UnityEngine;

[System.Serializable]
public class PlayerWeapon
{
    // if more weapons will be implemented, this parameters will be "changeable"
    public string name = "";
    public int damage = 10;
    public float range = 100f;

    public float fireRate = 0f;

    [SerializeField]
    public int maxBullets;

    [SerializeField]
    public int bullets;

    public float reloadTime = 5f;

    // construtor
    public PlayerWeapon()
    {
        bullets = maxBullets;
    }

    public GameObject graphics;
}
