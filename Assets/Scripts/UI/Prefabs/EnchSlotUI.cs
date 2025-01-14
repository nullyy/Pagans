﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnchSlotUI : MonoBehaviour
{
    public UnityEngine.UI.Text nameTxt;

    public Rune runeItem;
    public Dust dustItem;
    public ItemBase gemItem;

    public void SetData(Rune slot)
    {
        runeItem = slot;
        nameTxt.text = slot.Name;
    }

    public void SetData(Dust slot)
    {
        dustItem = slot;
        nameTxt.text = slot.Name;
    }

    public void SetData(ItemBase gem)
    {
        gemItem = gem;
        nameTxt.text = gem.Name;
    }
}
