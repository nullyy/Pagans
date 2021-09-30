﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public bool isActive;

    public string title;
    public string description;

    public int experienceReward;
    public int goldReward;

    public QuestGoal goal;

    public void Complete()
    {
        GameController.Instance.player.experience += experienceReward;
        GameController.Instance.player.gold += goldReward;
        GameController.Instance.player.quest = null;
        GameController.Instance.player.UpdateQuestUI();
        GameController.Instance.player.QuestsContainer.OnComplete(this);

        isActive = false;
    }
}
