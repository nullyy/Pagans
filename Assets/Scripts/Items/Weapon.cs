﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Weapon/generic weapon")]
public class Weapon : ItemBase
{
    public override void Use(Player player)
    {
        player.Attack(this);
    }

    public override void Use(Player player, IEntity npc, int damage = 1)
    {
        
    }
}
