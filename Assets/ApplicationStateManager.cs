using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Menus;

public class ApplicationStateManager
{
    private ApplicationState m_currentApplicationState;

    private Renderers m_renderers;



    static public ApplicationStateManager GetInstance()
    {
        if (m_instance == null)
            m_instance = new ApplicationStateManager();
        return m_instance;
    }

    static private ApplicationStateManager m_instance;

    private ApplicationStateManager()
    {
        m_currentApplicationState = null;//MainMenu.CreateMainMenu(m_renderers.MainMenuR);
    }

    public void UpdateCurrentState(Inputs _inputs)
    {
        if(m_currentApplicationState == null)
        {
            m_currentApplicationState = MainMenu.CreateMainMenu(m_renderers.MainMenuR);
        }
        m_currentApplicationState.Update(_inputs);
    }

    public void InitManager(Renderers _renderers)
    {
        m_renderers = _renderers;
    }

    private void SetCurrentApplicationState(ApplicationState _changeTo)
    {
        if(m_currentApplicationState!=null)
            m_currentApplicationState.StateRenderer.DisableRendering();
        m_currentApplicationState = _changeTo;
        m_currentApplicationState.StateRenderer.EnableRendering();
    }

    public void SetMainMenu()
    {
        SetCurrentApplicationState(MainMenu.CreateMainMenu(m_renderers.MainMenuR));
        GameManager.Instance.SoundManager.SetMenuAudio();
    }

    public void SetCharacterSelectScreen(Match.SetData _setData)
    {
        SetCurrentApplicationState(CharacterSelectScreen.GetCharacterSelectScreen(_setData, m_renderers.CharacterSelectR));
        GameManager.Instance.SoundManager.SetMenuAudio();
    }

    public void SetGameplayState(Match.SetData _setData)
    {
        SetCurrentApplicationState(Gameplay.GameplayState.CreateGameplayState(_setData, m_renderers.GameplayR, GameManager.Instance.IsOnlineMatch));
        GameManager.Instance.SoundManager.SetGameplayAudio();
    }

    /// <summary>
    /// Gets the application state, only for AI use.
    /// </summary>
    /// <returns></returns>
    internal ApplicationState GetApplicationState()
    {
        return m_currentApplicationState;
    }


    //for debugging
    public static string GetCurrentStateName()
    {
        return GetInstance().m_currentApplicationState.GetDebugInfo();

        ApplicationState ass = GetInstance().m_currentApplicationState;
        if (ass is MainMenu)
            return "MAIN MENU";
        if (ass is CharacterSelectScreen)
            return "CHARACTER SELECT SCREEN";
        if (ass is Gameplay.GameplayState)
            return "GAMEPLAY";
        return "UNDEFINED";
    }
}