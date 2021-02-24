using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private Portal[] portals;

    void Awake(){
        portals = FindObjectsOfType<Portal>();
    }

    void OnPreCull(){
        foreach(Portal portal in portals)
            portal.screen.enabled = false;

        foreach(Portal portal in portals)
            portal.Render();

        foreach(Portal portal in portals)
            portal.screen.enabled = true;
    }
}
