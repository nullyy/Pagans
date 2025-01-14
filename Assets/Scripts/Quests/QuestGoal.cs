﻿using UnityEngine;
using MyBox;

public enum GoalType
{
    KillTot,
    KillSomeone,
    Talk,
    Buy,
    Sell,
    EnterADoor,
    GetItem,
    BuyWeapon,
    EquipItem
}

[System.Serializable]
public class QuestGoal
{
    public float dialogueDelay = 1f;
    public Dialogue introDialogue;

    public string goal;

    public GoalType goalType;

    [ConditionalField(nameof(goalType), false, GoalType.KillTot)] public int requiredAmount;
    [ConditionalField(nameof(goalType), false, GoalType.KillTot)] public int currentAmount;

    [ConditionalField(nameof(goalType), false, GoalType.KillSomeone)] public string enemyName;

    [ConditionalField(nameof(goalType), false, GoalType.Talk)] public string talkTo;

    [ConditionalField(nameof(goalType), false, new object[] { GoalType.Buy, GoalType.Sell })] [SerializeField] string sellerName;
    [ConditionalField(nameof(goalType), false, GoalType.Buy)] [SerializeField] string itemName;

    [ConditionalField(nameof(goalType), false, GoalType.EnterADoor)] [SerializeField] string PortalName;

    [ConditionalField(nameof(goalType), false, new object[] { GoalType.GetItem, GoalType.Sell })] [SerializeField] ItemBase GoalItem;

    [ConditionalField(nameof(goalType), false, GoalType.EquipItem)] [SerializeField] ItemBase itemToEquip;

    void Complete()
    {
        var quest = Player.i.quest;
        if (quest.goal.Count == 1)
        {
            quest.goal.RemoveAt(0);
            quest.Complete();
            Player.i.UpdateQuestUI();
        }
        else if (quest.goal.Count >= 2)
        {
            Player.i.StartCoroutine(GameController.Instance.EvH.GoalCompleted(quest));
        }
    }

    public void EnemyKilled(NPCController enemy)
    {
        if(goalType == GoalType.KillTot)
            currentAmount++;
        else if(goalType == GoalType.KillSomeone)
        {
            if (enemy.Name == enemyName)
                Complete();
        }
    }

    public void NPCTalked(NPCController npc)
    {
        if(goalType == GoalType.Talk)
        {
            if (npc.Name == talkTo)
                Complete();
        }
    }

    public void DoorEntered(Portal door) // viene passata la destinazione
    {
        if (door.name == PortalName && goalType==GoalType.EnterADoor)
            Complete();
    }

    public void SomethingBought(TraderController seller, ItemBase item)
    {
        if(seller.Name == sellerName && item.Name == itemName && goalType == GoalType.Buy)
        {
            Complete();
        }

        if(goalType == GoalType.BuyWeapon)
        {
            if (item is Weapon)
                Complete();
        }
    }

    public void SomethingSelled(TraderController buyer, ItemBase merch)
    {
        if(merch.Name == GoalItem.Name && goalType == GoalType.Sell)
        {
            if (sellerName != null && sellerName != "") // seller in questo caso fa da buyer, sti cazzi del nome non mi va di fare troppe vars
            {
                if (sellerName == buyer.Name)
                    Complete();
            }
            else
                Complete();
        }
    }

    public void SomethingAddedToInventory(ItemBase addedItem)
    {
        if (addedItem.Name == GoalItem.Name && goalType == GoalType.GetItem)
            Complete();
    }

    public void SomethingEquiped(ItemBase item)
    {
        if(goalType == GoalType.EquipItem)
        {
            if (item.Name == itemToEquip.Name)
            {
                Complete();
            }
        }
    }
}