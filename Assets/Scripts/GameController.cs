﻿using System.Collections;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
public enum GameState {
    FreeRoam,
    Menu, 
    Settings,
    Dialogue, 
    Quest, 
    Inventory,
    Equipment, 
    Shop, 
    Quests, 
    Enchanting,
    Battle, 
    ChoosingItem,
    Library,
    GameOver,
    Cauldron,
    CraftUI,
    Workbench,
    Port,
    Calendar
};

public class GameController : MonoBehaviour
{
    public GameObject EssentialObjectsPrefab;

    public Player player;
    [SerializeField] public MenuController menu;
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] public ShopUI shopUI;
    [SerializeField] QuestsUI questsUI;
    [SerializeField] EnchantingUI enchantingUI;
    [SerializeField] public Hotbar hotbar;
    [SerializeField] public SettingsUI settingsUI;
    [SerializeField] Volume ppv; // post processing volume
    [SerializeField] GameObject BattleScene;
    [SerializeField] public ChoosingUI choosingUI;
    [SerializeField] public GameObject MinimapCanvas;
    [SerializeField] public bool LaunchStory;
    [SerializeField] public doorKeyUI keyUI;
    [SerializeField] public GameObject sparksParticle;
    [SerializeField] public newInventory inventory2;
    [SerializeField] public LibUI libUI;
    [SerializeField] GameOverUI gameOverUI;
    [SerializeField] public CauldronUI cauldronUI;
    public CraftUI craftUI;
    public WorkbenchUI workbenchUI;
    public PortUI portUI;
    public Calendar calendar;

    [SerializeField] bool ResetOnEnd = false;

    float tick=60, seconds=0, mins=0, hours=11, day = 1, month=1, year=1248;
    [SerializeField, Range(0, 24)] float TimeOfDay;
    bool activateLights;

    public DialogueManager dialogueBox;
    public QuestController questWindow;
    public NewItemUI newItemUI;
    public SacrificeUI sacrificeUI;

    public StoryEventHandler EvH;

    public GameObject keytip_E;
    public GameObject keytip_Z;
    public GameObject keytip_F;

    // basic
    public Color selectedDefaultColor;
    public Color unselectedDefaultColor = Color.black;
    public Color selectedOnBookColor;
    // firsts
    public Color equipedDefaultColor;
    public Color equipedSelectedColor;
    // secondaries
    public Color secondaryDefaultColor;
    public Color secondarySelectedColor;
    // UI
    public Color AffordableGreenColor;
    public Color UnaffordableRedColor;

    [HideInInspector] public GameState state = GameState.FreeRoam;
    [HideInInspector] public NPCController ActiveNPC;
    [HideInInspector] public StoryController storyController;

    [SerializeField] Quest talkToHarbardQuest;

    [SerializeField] Color eveningLightsColor;
    [SerializeField] Gradient nightLightsColor;

    public static GameController Instance;

    float timer = 10f;

    public EnchantingUI EnchUI => enchantingUI;

    private void Awake()
    {
        Instance = this;
        storyController = GetComponent<StoryController>();
    }

    private void Start()
    {
        //hotbar.UpdateItems();
        MinimapCanvas.SetActive(false);
        ppv = gameObject.GetComponent<Volume>();
        EvH = FindObjectOfType<StoryEventHandler>();
        player.Load();
        if(storyController.firstLaunch)
        {
            dialogueBox.StartDialogue(new Dialogue(new string[] { "ciao, tu devi essere Ulfr.", "ti guiderò alla scoperta di pagans, diventa familiare con i comandi arrivando in biblioteca." }), () => 
            {
                player.QuestsContainer.Add(talkToHarbardQuest);
                state = GameState.FreeRoam;
            });
        }
    }

    private void OnDestroy()
    {
        print("destroying");
        SaveSystem.Reset();
        /*if (ResetOnEnd)
            SaveSystem.Reset();
        else
            player.Save();*/
    }

    private void FixedUpdate()
    {
        //CalcTime(); // unrem for day night [Bugged] cycle.
    }

    #region daynight
    void CalcTime() // Used to calculate sec, min and hours
    {
        seconds += Time.fixedDeltaTime * tick; // multiply time between fixed update by tick

        if (seconds >= 60) // 60 sec = 1 min
        {
            seconds = 0;
            mins += 1;
        }

        if (mins >= 60) //60 min = 1 hr
        {
            mins = 0;
            hours += 1;
        }

        if (hours >= 24) //24 hr = 1 day
        {
            hours = 0;
            day += 1;
        }
        ControlPPV(); // changes post processing volume after calculation
    }

    void ControlPPV()
    {
        ppv.weight = (float)mins / 60; // since dusk is 1 hr, we just divide the mins by 60 which will slowly increase from 0 - 1

        if (activateLights == false) // if lights havent been turned on
        {
            if (mins > 45) // wait until pretty dark
            {
                for (int i = 0; i < player.currentScene.lights.Length; i++)
                {
                    player.currentScene.lights[i].SetActive(true); // turn them all on
                }
                activateLights = true;
            }
        }

        if (hours >= 6 && hours < 7) // Dawn at 6:00 / 6am    -   until 7:00 / 7am
        {
            ppv.weight = 1 - (float)mins / 60; // we minus 1 because we want it to go from 1 - 0
            if (activateLights == true) // if lights are on
            {
                if (mins > 45) // wait until pretty bright
                {
                    for (int i = 0; i < player.currentScene.lights.Length; i++)
                    {
                        player.currentScene.lights[i].SetActive(false); // shut them off
                    }
                    activateLights = false;
                }
            }
        }
    }
    #endregion

    public void OpenState(GameState state, TraderController trader = null, Cauldron c=null, ItemBase craftItem=null)
    {
        print($"target state:{state}, trader passed:{trader}.");
        IdleAllEnemies();
        #region state control
        if (state == GameState.Menu)
        {
            menu.gameObject.SetActive(true);
        }
        else if (state == GameState.Inventory)
        {
            inventory2.UpdateView();
            inventory2.gameObject.SetActive(true);
        }
        else if (state == GameState.Shop && trader != null)
        {
            shopUI.SetTrader(trader); // Questo deve essere chiamato prima del setActive visto che quest'ultima funzione chiama anche Awake().
            shopUI.gameObject.SetActive(true);
        }
        else if (state == GameState.Quests)
        {
            questsUI.gameObject.SetActive(true);
            questsUI.UpdateContents();
        }
        else if (state == GameState.ChoosingItem)
        {
            choosingUI.gameObject.SetActive(true);
            choosingUI.UpdateItems();
            choosingUI.GetComponent<Animator>().SetTrigger("Anim");
        }
        else if (state == GameState.Settings)
        {
            settingsUI.gameObject.SetActive(true);
            menu.gameObject.SetActive(false);
            settingsUI.UpdateSelection();
        }
        else if (state == GameState.Library)
        {
            libUI.gameObject.SetActive(true);
            libUI.UpdateCategorySelection();
        }
        else if(state == GameState.GameOver)
        {
            gameOverUI.gameObject.SetActive(true);
        }
        else if(state == GameState.Cauldron)
        {
            cauldronUI.SetSource(c);
            cauldronUI.UpdateContents();
            cauldronUI.gameObject.SetActive(true);
        }
        else if(state == GameState.CraftUI)
        {
            craftUI.UpdateContents(craftItem);
            craftUI.gameObject.SetActive(true);
        }
        else if(state == GameState.Workbench)
        {
            workbenchUI.gameObject.SetActive(true);
            workbenchUI.UpdateContents();
        }
        else if(state == GameState.Port)
        {
            portUI.gameObject.SetActive(true);
            portUI.UpdateContents();
        }
        else if(state == GameState.Calendar)
        {
            calendar.gameObject.SetActive(true);
            calendar.UpdateContents();
        }
        else
            print("[!!] No state specified or unhandled option.");
        #endregion

        this.state = state;
    }

    #region battle stuffs
    public void StartBattle()
    {
        state = GameState.Battle;
        player.transform.GetChild(0).GetComponent<Camera>().enabled = false;
        player.gameObject.SetActive(false);
        BattleScene.gameObject.SetActive(true);
    }

    public void EndBattle()
    {
        BattleScene.gameObject.SetActive(false);
        player.transform.GetChild(0).GetComponent<Camera>().enabled = true;
        player.gameObject.SetActive(true);
        state = GameState.FreeRoam;
    }
    #endregion

    private void Update()
    {
        UpdateTime();
        if (state != GameState.FreeRoam)
        {
            player.animator.SetFloat("Speed", 0.0f);
            player.moveInput = Vector2.zero;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            player.canShowMinimap = false;
        }
        #region update choose
        // fast switching: inventory -> equipment -> quests -> inventory.

        if (state == GameState.FreeRoam)
        {
            if (!player.canShowMinimap)
                player.canShowMinimap = true;

            UpdateEnemiesInViewport();
            player.HandleUpdate();
        }

        else if (state == GameState.GameOver)
        {
            gameOverUI.HandleUpdate();
        }

        else if (state == GameState.Cauldron)
            cauldronUI.HandleUpdate();

        else if (state == GameState.Calendar)
            calendar.HandleUpdate();

        else if (state == GameState.Workbench)
            workbenchUI.HandleUpdate();

        else if (state == GameState.CraftUI)
            craftUI.HandleUpdate();

        else if (state == GameState.Port)
            portUI.HandleUpdate();

        else if (state == GameState.Menu)
        {
            Action onBack = () =>
            {
                menu.gameObject.SetActive(false);
                state = GameState.FreeRoam;
            };
            menu.HandleUpdate(onBack);
        }

        else if (state == GameState.Dialogue)
            dialogueBox.HandleUpdate();

        else if (state == GameState.Quest)
            questWindow.HandleUpdate();

        else if (state == GameState.Inventory)
        {
            /*inventoryUI.HandleUpdate();
            if(Input.GetKeyDown(KeyCode.Tab) || Input.GetButtonDown("RShoulder"))
            {
                inventoryUI.gameObject.SetActive(false);
                OpenState(GameState.Equipment);
            }*/
            inventory2.HandleUpdate();
        }

        else if (state == GameState.Shop)
            shopUI.HandleUpdate();

        else if (state == GameState.Quests)
        {
            questsUI.HandleUpdate();

            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetButtonDown("RShoulder"))
            {
                questsUI.gameObject.SetActive(false);
                OpenState(GameState.Inventory);
            }
        }

        else if (state == GameState.ChoosingItem)
        {
            choosingUI.HandleUpdate();
        }

        else if (state == GameState.Enchanting)
        {
            enchantingUI.HandleUpdate();
        }

        else if (state == GameState.Settings)
        {
            Action onBack = () =>
            {
                settingsUI.gameObject.SetActive(false);
                OpenState(GameState.Menu);
            };
            settingsUI.HandleUpdate(onBack);
        }

        else if (state == GameState.Library)
        {
            libUI.HandleUpdate();
        }
        #endregion
    }

    void UpdateTime()
    {
        mins += 10*Time.deltaTime;
        if(mins >= 60)
        {
            mins = 0;
            hours++;
            if (hours >= 24)
            {
                hours = 0;
                day++;
                if(day >= 31)
                {
                    day = 1;
                    month++;
                }
            }
        }

        var scaleTime = hours+(((10 * mins) / 6) / 100);

        if (player.currentScene.outdoor)
        {
            player.currentScene.light.color = nightLightsColor.Evaluate(hours / 24);
            // example: 22h 30m = 22.5f
            player.currentScene.light.intensity = Mathf.Clamp((Mathf.Sin((scaleTime / 3.8f) - 1.58f) + 1.2f) / 2, 0.1f, 1);
        }

        /*//player.currentScene.light.intensity = Mathf.Clamp(hours/10, 0.1f, 1);
        // interpolate yellow
        int targetHour = 18;
        //60 * (targetHour - hours)
        player.currentScene.light.color = Color.Lerp(player.currentScene.light.color, nightLightsColor, Time.deltaTime/timer);
        timer -= Time.deltaTime;
        TimeOfDay += Time.deltaTime;
        TimeOfDay %= 24;
        // update using time/24f as timePercent
        player.currentScene.light.color = nightLightsColor.Evaluate(TimeOfDay / 24f);*/

    }

    void UpdateEnemiesInViewport()
    {
        foreach(EnemyController npc in FindObjectsOfType<EnemyController>())
        {
            Vector3 viewPos = UnityEngine.Camera.main.WorldToViewportPoint(npc.transform.position);
            if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
            {
                npc.HandleUpdate(); // update only if is visible.
            }
        }
    }

    void IdleAllEnemies() // this is heavy
    {
        foreach(EnemyController enemy in FindObjectsOfType<EnemyController>())
        {
            enemy.GetComponent<Animator>().SetFloat("speed", 0f);
        }
    }

    public void ShowMessage(string msg)
    {
        ShowMessage(new string[] { msg });
    }

    public void ShowMessage(string[] msgs)
    {
        var prevState = state;
        dialogueBox.StartDialogue(new Dialogue(msgs), () => { state = prevState; });
    }

    public void ShowInfo(string text, Action onEndDialogue, float duration = 1f)
    {
        StartCoroutine(dialogueBox.InfoDialogue(new Dialogue(new string[] { text }), duration, onEndDialogue));
    }
}
