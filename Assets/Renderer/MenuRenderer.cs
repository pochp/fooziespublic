using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class MenuRenderer : MonoBehaviour, IRenderer
{
    [SerializeField]
    private GameObject m_menuCanvas;

    public MenuRenderer(GameObject _menuCanvas)
    {
        m_menuCanvas = _menuCanvas;
    }

    public void DisableRendering()
    {
        m_menuCanvas.SetActive(false);
    }

    public void EnableRendering()
    {
        m_menuCanvas.SetActive(true);
    }
}