using Assets.Match;
using Assets.States;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class GameplayRenderer : MonoBehaviour, IRenderer {

    public float CharacterHeight = 1.8f;

    public Material P1_Active;
    public Material P1_Character;
    public Material P1_Recovery;
    public Material P1_Startup;
    public Material P2_Active;
    public Material P2_Character;
    public Material P2_Recovery;
    public Material P2_Startup;

    public Sprite TXT_1;
    public Sprite TXT_2;
    public Sprite TXT_3;
    public Sprite TXT_Foozies;
    public Sprite TXT_Counterhit;
    public Sprite TXT_Draw;
    public Sprite TXT_Player1;
    public Sprite TXT_Player2;
    public Sprite TXT_Shimmy;
    public Sprite TXT_StrayHit;
    public Sprite TXT_Win;
    public Sprite TXT_WhiffPunish;
    public Sprite TXT_Trade;
    public Sprite TXT_Timeout;
    public Sprite TXT_Throw;
    public Sprite TXT_GameOver;
    public Sprite TXT_AnyButton;
    public Sprite TXT_Feet;
    public Sprite TXT_Punish;
    public Sprite TXT_CharSelect;
    public Sprite TXT_MainMenu;

    public GameObject GameplayCanvas;
    public Text P1_CharName;
    public Text P2_CharName;
    public Text Timer;

    private List<Hitbox_Render> m_activeHitboxes;
    private SpriteList m_spriteList;

    private MatchState m_previousMatchState;
    private SplashState m_previousSplashState;

    private GaugeRenderer GaugeRenderer;

    // Use this for initialization
    void Start()
    {
        m_activeHitboxes = new List<Hitbox_Render>();
        m_previousMatchState = new MatchState();
        m_previousSplashState = new SplashState();
        InitSprites();
    }

    private void InitSprites()
    {
        m_spriteList = new SpriteList(this);
        GaugeRenderer = new GaugeRenderer(P1_Active, P1_Character);
    }

    public void RenderScene(GameState _gameState, MatchState _matchState, SplashState _splashState)
    {
        //1. Render Hitboxes
        RenderHitboxes(_gameState);

        //2. Render Effects (hit sparks)

        //3. Render Score, Time, Gauges
        GaugeRenderer.UpdateGaugeTransforms(_gameState);

        //4. Splash situations (match end, round end)
        RenderSplashSprites(_gameState, _matchState, _splashState);

        //5. Labels
        RenderLabels(_gameState, _matchState, _splashState);
    }

    public void RenderLabels(GameState _gameState, MatchState _matchState, SplashState _splashState)
    {
        P1_CharName.text = _gameState.P1_CState.GetCharacterName() + " - " + _matchState.P1_Score.ToString();
        P2_CharName.text = _gameState.P2_CState.GetCharacterName() + " - " + _matchState.P2_Score.ToString();
        Timer.text = (_gameState.RemainingTime/60).ToString();
    }

    public void RenderSplashSprites(GameState _gameState, MatchState _matchState, SplashState _splashState)
    {
        //check if there is a change
        if(m_previousSplashState.CurrentState != _splashState.CurrentState)
        {
            //1. deactivate previous sprites
            foreach(SpriteRenderPair pair in m_spriteList.SplashSprites)
            {
                pair.Active = false;
                pair.SpriteRenderer.sprite = null;
            }

            //2. activate required sprites
            switch(_splashState.CurrentState)
            {
                case SplashState.State.RoundOver_ShowResult:
                    RenderMatchResult( _matchState);
                    break;
                case SplashState.State.RoundStart_1:
                    m_spriteList.TXT_1.Active = true;
                    break;
                case SplashState.State.RoundStart_2:
                    m_spriteList.TXT_2.Active = true;
                    break;
                case SplashState.State.RoundStart_3:
                    m_spriteList.TXT_3.Active = true;
                    break;
                case SplashState.State.RoundStart_F:
                    m_spriteList.TXT_Foozies.Active = true;
                    break;
                case SplashState.State.GameOver:
                    m_spriteList.TXT_GameOver.Active = true;
                    m_spriteList.TXT_AnyButton.Active = true;
                    break;
            }
            //render what has to be rendered
            foreach (SpriteRenderPair pair in m_spriteList.SplashSprites)
            {
                if (pair.Active)
                    pair.SpriteRenderer.sprite = pair.Sprite;
            }
        }


        m_previousSplashState = new SplashState(_splashState);
    }

    private void RenderMatchResult(MatchState _matchState)
    {
        if (_matchState.Outcomes.Count <= 0)
            return;
        MatchOutcome lastOutcome = _matchState.Outcomes[_matchState.Outcomes.Count - 1];
        if (lastOutcome.P1_Scores && lastOutcome.P2_Scores)
        {
            m_spriteList.TXT_Draw.Active = true;
        }
        else if (lastOutcome.P1_Scores)
        {
            m_spriteList.TXT_Player1.Active = true;
            m_spriteList.TXT_Win.Active = true;
        }
        else if (lastOutcome.P2_Scores)
        {
            m_spriteList.TXT_Player2.Active = true;
            m_spriteList.TXT_Win.Active = true;
        }

        switch(lastOutcome.Outcome)
        {
            case GameplayEnums.Outcome.Counter:
                m_spriteList.TXT_Counterhit.Active = true;
                break;
            case GameplayEnums.Outcome.Shimmy:
                m_spriteList.TXT_Shimmy.Active = true;
                break;
            case GameplayEnums.Outcome.StrayHit:
                m_spriteList.TXT_StrayHit.Active = true;
                break;
            case GameplayEnums.Outcome.Throw:
                m_spriteList.TXT_Throw.Active = true;
                break;
            case GameplayEnums.Outcome.TimeOut:
                m_spriteList.TXT_Timeout.Active = true;
                break;
            case GameplayEnums.Outcome.Trade:
                m_spriteList.TXT_Trade.Active = true;
                break;
            case GameplayEnums.Outcome.WhiffPunish:
                m_spriteList.TXT_WhiffPunish.Active = true;
                break;
            case GameplayEnums.Outcome.Sweep:
                m_spriteList.TXT_Feet.Active = true;
                break;
            case GameplayEnums.Outcome.Punish:
                m_spriteList.TXT_Punish.Active = true;
                break;
        }
    }

    public void RenderHitboxes(GameState _gameState)
    {
        //1. to make sure hitboxes that don't exist any more are deleted :
        foreach(Hitbox_Render hbr in m_activeHitboxes)
        {
            hbr.StillExists = false;
        }

        //2. p1 boxes
        foreach (Hitbox_Gameplay gb_gp in _gameState.P1_Hitboxes)
        {
            RenderHitbox(gb_gp, true, _gameState);
        }
        //3. p2 boxes
        foreach (Hitbox_Gameplay gb_gp in _gameState.P2_Hitboxes)
        {
            RenderHitbox(gb_gp, false, _gameState);
        }

        //4. delete non existing boxes
        foreach (Hitbox_Render hbr in m_activeHitboxes)
        {
            if (!hbr.StillExists)
                Destroy(hbr.ObjectInGame);
        }
        m_activeHitboxes.RemoveAll(o => !o.StillExists);
    }

    public void RenderHitbox(Hitbox_Gameplay _hbox_gameplay, bool _p1, GameState _gameState)
    {
        //1. if an existing render hitbox exists for it, simply modify it
        bool found = false;
        foreach (Hitbox_Render renderBox in m_activeHitboxes)
        {
            if(renderBox.ReferenceBox.ID == _hbox_gameplay.ID)
            {
                ModifyHitbox(renderBox, _hbox_gameplay, _p1, _gameState);
                found = true;
            }
        }

        //2. if one does not, create a new one
        if(!found)
        {
            CreateNewHitbox(_hbox_gameplay, _p1, _gameState);
        }
    }

    private void ModifyHitbox(Hitbox_Render _renderBox, Hitbox_Gameplay _hbox_gameplay, bool _p1, GameState _gameState)
    {
        float y = CharacterHeight;
        float z = 1;
        if (_hbox_gameplay.HitboxType == GameplayEnums.HitboxType.Hitbox_Attack)
        {
            y /= 2f;
            z = 2;
        }
        
        Vector3 pos = GetInGamePosition(_hbox_gameplay, _p1, _gameState);
        Vector3 scale = new Vector3(GameUnitsToUnityUnits(_hbox_gameplay.Width), y, z);
        _renderBox.ObjectInGame.transform.position = pos;
        _renderBox.ObjectInGame.transform.localScale = scale;
        if (_hbox_gameplay.AttackAttribute == GameplayEnums.AttackAttribute.Low)
        {
            _renderBox.ObjectInGame.transform.position = pos - new Vector3(0, y / 2f, 2f);
            //_renderBox.ObjectInGame.transform.localScale = new Vector3(scale.x, scale.y / 2f);
        }
        _renderBox.StillExists = true;
    }

    private void CreateNewHitbox(Hitbox_Gameplay _hbox_gameplay, bool _p1, GameState _gameState)
    {
        Hitbox_Render _renderBox = new Hitbox_Render();
        _renderBox.ReferenceBox = _hbox_gameplay;
        _renderBox.ObjectInGame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ModifyHitbox(_renderBox, _hbox_gameplay, _p1, _gameState);
        switch(_hbox_gameplay.HitboxType)
        {
            case GameplayEnums.HitboxType.Hitbox_Attack:
            case GameplayEnums.HitboxType.Hitbox_Throw:
                if(_p1)
                    _renderBox.ObjectInGame.GetComponent<Renderer>().material = P1_Active;
                else
                    _renderBox.ObjectInGame.GetComponent<Renderer>().material = P2_Active;
                break;
            case GameplayEnums.HitboxType.Hurtbox_Limb:
            case GameplayEnums.HitboxType.Hurtbox_Main:
                GameplayEnums.CharacterState currentCharacterState;
                if (_p1)
                    currentCharacterState = _gameState.P1_State;
                else
                    currentCharacterState = _gameState.P2_State;
                Material toSet;
                GameplayEnums.CharacterState cstate = currentCharacterState;
                if (cstate == GameplayEnums.CharacterState.Special)
                {
                    if (_p1)
                        cstate = _gameState.P1_CState.SelectedCharacter.GetEquivalentState();
                    else
                        cstate = _gameState.P2_CState.SelectedCharacter.GetEquivalentState();
                }
                switch(cstate)
                {
                    case GameplayEnums.CharacterState.AttackActive:
                    case GameplayEnums.CharacterState.AttackStartup:
                    case GameplayEnums.CharacterState.ThrowActive:
                    case GameplayEnums.CharacterState.ThrowStartup:
                        if (_p1)
                            toSet = P1_Startup;
                        else
                            toSet = P2_Startup;
                        break;
                    case GameplayEnums.CharacterState.AttackRecovery:
                    case GameplayEnums.CharacterState.Blockstun:
                    case GameplayEnums.CharacterState.Clash:
                    case GameplayEnums.CharacterState.ThrowBreak:
                    case GameplayEnums.CharacterState.ThrowRecovery:
                        if (_p1)
                            toSet = P1_Recovery;
                        else
                            toSet = P2_Recovery;
                        break;
                    default:
                        if (_p1)
                            toSet = P1_Character;
                        else
                            toSet = P2_Character;
                        break;
                }
                _renderBox.ObjectInGame.GetComponent<Renderer>().material = toSet;
                break;
        }
        m_activeHitboxes.Add(_renderBox);
    }

    private Vector3 GetInGamePosition(Hitbox_Gameplay _hbox_gameplay, bool _p1, GameState _gameState)
    {
        int position;
        if (_p1)
        {
            position = _gameState.P1_Position + _hbox_gameplay.Position;
        }
        else
        {
            position = _gameState.P2_Position + _hbox_gameplay.Position;
        }
        return GameCoordsToUnityCoords(position);
    }

    public Vector3 GameCoordsToUnityCoords(int x)
    {
        return new Vector3(GameUnitsToUnityUnits(x), 0f, 0f);
    }

    public float GameUnitsToUnityUnits(int x)
    {
        return (float)x / 1000f;
    }

    public void DisableRendering()
    {
        foreach(Hitbox_Render hr in m_activeHitboxes)
        {
            Destroy(hr.ObjectInGame);
        }
        m_spriteList.HideAll();
        GameplayCanvas.SetActive(false);
    }

    public void EnableRendering()
    {
        GameplayCanvas.SetActive(true);
    }
}

public class Hitbox_Render
{
    public Hitbox_Gameplay ReferenceBox;
    public GameplayEnums.HitboxType HitboxType;
    public int Width;
    public int Position;
    public GameObject ObjectInGame;
    public bool StillExists;
}

public class SpriteRenderPair
{
    public Sprite Sprite;
    public SpriteRenderer SpriteRenderer;
    public bool Active;
    public GameObject ChildObject;
    private SpriteRenderPair()
    {
        Active = false;
    }

    static public SpriteRenderPair CreateRenderPair(GameObject _gameObject, Sprite _s, float _positionOnScreen)
    {
        SpriteRenderPair srp = new SpriteRenderPair();
        srp.ChildObject = new GameObject(_s.name + "Renderer");
        srp.SpriteRenderer = srp.ChildObject.AddComponent<SpriteRenderer>() as SpriteRenderer;
        srp.Sprite = _s;
        srp.ChildObject.transform.SetParent(_gameObject.transform);
        srp.ChildObject.transform.localPosition = new Vector3(0, _positionOnScreen, -0.1f);
        return srp;
    }
}

public class SpriteList
{
    public SpriteRenderPair TXT_1;
    public SpriteRenderPair TXT_2;
    public SpriteRenderPair TXT_3;
    public SpriteRenderPair TXT_Foozies;
    public SpriteRenderPair TXT_Counterhit;
    public SpriteRenderPair TXT_Draw;
    public SpriteRenderPair TXT_Player1;
    public SpriteRenderPair TXT_Player2;
    public SpriteRenderPair TXT_Shimmy;
    public SpriteRenderPair TXT_StrayHit;
    public SpriteRenderPair TXT_Win;
    public SpriteRenderPair TXT_WhiffPunish;
    public SpriteRenderPair TXT_Trade;
    public SpriteRenderPair TXT_Timeout;
    public SpriteRenderPair TXT_Throw;
    public SpriteRenderPair TXT_GameOver;
    public SpriteRenderPair TXT_AnyButton;
    public SpriteRenderPair TXT_Feet;
    public SpriteRenderPair TXT_Punish;


    public List<SpriteRenderPair> SplashSprites;
    public void HideAll()
    {
        foreach(var v in SplashSprites)
        {
            v.Active = false;
        }
    }

    public SpriteList(GameplayRenderer _gameplayRenderer)
    {
        SplashSprites = new List<SpriteRenderPair>();
        float ScreenTop = 3;
        float ScreenMiddle = 0;
        float ScreenBottom = -3;
        TXT_1 = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_1, ScreenMiddle);
        TXT_2 = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_2, ScreenMiddle);
        TXT_3 = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_3, ScreenMiddle);
        TXT_Foozies = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Foozies, ScreenMiddle);
        TXT_Counterhit = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Counterhit, ScreenBottom);
        TXT_Draw = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Draw, ScreenMiddle);
        TXT_Player1 = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Player1, ScreenTop);
        TXT_Player2 = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Player2, ScreenTop);
        TXT_Shimmy = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Shimmy, ScreenBottom);
        TXT_StrayHit = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_StrayHit, ScreenBottom);
        TXT_Win = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Win, ScreenMiddle);
        TXT_WhiffPunish = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_WhiffPunish, ScreenBottom);
        TXT_Trade = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Trade, ScreenBottom);
        TXT_Timeout = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Timeout, ScreenBottom);
        TXT_Throw = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Throw, ScreenBottom);
        TXT_GameOver = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_GameOver, ScreenTop);
        TXT_AnyButton = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_AnyButton, ScreenBottom);
        TXT_Feet = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Feet, ScreenBottom);
        TXT_Punish = SpriteRenderPair.CreateRenderPair(_gameplayRenderer.gameObject, _gameplayRenderer.TXT_Punish, ScreenBottom);

        SplashSprites.Add(TXT_1);
        SplashSprites.Add(TXT_2);
        SplashSprites.Add(TXT_3);
        SplashSprites.Add(TXT_Foozies);
        SplashSprites.Add(TXT_Counterhit);
        SplashSprites.Add(TXT_Draw);
        SplashSprites.Add(TXT_Player1);
        SplashSprites.Add(TXT_Player2);
        SplashSprites.Add(TXT_Shimmy);
        SplashSprites.Add(TXT_StrayHit);
        SplashSprites.Add(TXT_Win);
        SplashSprites.Add(TXT_WhiffPunish);
        SplashSprites.Add(TXT_Trade);
        SplashSprites.Add(TXT_Timeout);
        SplashSprites.Add(TXT_Throw);
        SplashSprites.Add(TXT_GameOver);
        SplashSprites.Add(TXT_AnyButton);
        SplashSprites.Add(TXT_Feet);
        SplashSprites.Add(TXT_Punish);
    }
}