using Assets.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class GaugeRenderer
{
    GameObject P1_Gauge;
    GameObject P1_GaugeBackground;
    GameObject P2_Gauge;
    GameObject P2_GaugeBackground;

    public GaugeRenderer(Material _value, Material _background)
    {
        P1_GaugeBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        P1_GaugeBackground.GetComponent<Renderer>().material = _background;
        SetBackgroundTransform(P1_GaugeBackground, true);
        P2_GaugeBackground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        P2_GaugeBackground.GetComponent<Renderer>().material = _background;
        SetBackgroundTransform(P2_GaugeBackground, false);
        P1_Gauge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        P1_Gauge.GetComponent<Renderer>().material = _value;
        P2_Gauge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        P2_Gauge.GetComponent<Renderer>().material = _value;
    }

    Vector3 GetBackgroundPosition(bool _p1)
    {
        if (_p1)
            return new Vector3(-3, -3, -1);
        else
            return new Vector3(3, -3, -1);
    }

    Vector3 GetBackgroundScale()
    {
        return new Vector3(3, 1, 1);
    }

    void SetBackgroundTransform(GameObject _obj, bool _p1)
    {
        _obj.transform.position = GetBackgroundPosition(_p1);
        _obj.transform.localScale = GetBackgroundScale();
    }

    public void UpdateGaugeTransforms(GameState m_gameState)
    {
        float maxgauge = 4f;

        int gauge1 = m_gameState.P1_Gauge;
        //scale from 0 to 2
        float xscale1 = (gauge1 / maxgauge) * 3f;
        //place from 1 to 3, but centered, so gauge(0) = 4.5, gauge(4) = 3
        float xpos1 = -(4.5f - gauge1 * 1.5f / maxgauge);
        P1_Gauge.transform.position = new Vector3(xpos1, -3f);
        P1_Gauge.transform.localScale = new Vector3(xscale1, 1,1);
        //has to show the opposite, so gauge(0) = pos(-3), scale(3), gauge(4) = pos(-1.5) scale(0)
        float bgxpos1 = -3f + gauge1 * 1.5f / maxgauge;
        P1_GaugeBackground.transform.position = new Vector3(bgxpos1, -3f);
        P1_GaugeBackground.transform.localScale = new Vector3(3f-xscale1, 1f);

        int gauge2 = m_gameState.P2_Gauge;
        //scale from 0 to 2
        float xscale2 = (gauge2 / maxgauge) * 3f;
        //place from 1 to 3, but centered, so gauge(0) = 4.5, gauge(4) = 3
        float xpos2 = (4.5f - gauge2 * 1.5f / maxgauge);
        P2_Gauge.transform.position = new Vector3(xpos2, -3f);
        P2_Gauge.transform.localScale = new Vector3(xscale2, 1,1);
        //has to show the opposite, so gauge(0) = pos(-3), scale(3), gauge(4) = pos(1.5) scale(0)
        float bgxpos2 = 3f - gauge2 * 1.5f / maxgauge;
        P2_GaugeBackground.transform.position = new Vector3(bgxpos2, -3f);
        P2_GaugeBackground.transform.localScale = new Vector3(3f - xscale2, 1f);
    }
}