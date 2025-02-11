﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotUI : MonoBehaviour
{
    public UnityEngine.UI.Text nameTxt;
    public UnityEngine.UI.Text countText;
    public ItemBase item;
    public int count;

    public void SetData(InventorySlot slot)
    {
        item = slot.item;
        count = slot.count;
        nameTxt.text = item.Name;
        countText.text = $"X{slot.count}";
    }
}
