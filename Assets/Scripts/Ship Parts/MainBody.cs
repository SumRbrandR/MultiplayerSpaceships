using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainBody : ShipPart
{
    [SerializeField]
    private PredictionMotor root;

    [SerializeField]
    MainMenu menu;
    CameraDampener cam;

    
    public override void OnShipCreated(PredictionMotor ship)
    {
        print("Body OnShipCreated");
        root = ship;

        menu = root.mainMenu;
        print("body menu = " + menu.name + "on awake");

        cam = GetComponentInChildren<CameraDampener>();
    }

   // [ServerRpc(RequireOwnership = false)]
    public override void DestroyIfDead()
    {
        //ChangeCounterpartColor(damageHudCounterpart, this);

        if (hitPoints <= 0f)
        {
            if (!hasRun)
                MainBodyDestroyIfDeadObservers();
        }
    }
    [ObserversRpc]
    void ChangeCamera()
    {
        Camera cam = GetComponentInChildren<Camera>();
        if (cam != null)
        {
            if(cam.transform.parent.name != "Cockpit")
            {
                print("body menu = " + menu.name + "when destroyed");

                menu.shipDestroyed = true;

                FindObjectOfType<IdleCamera>(true)?.gameObject.SetActive(true) ;

            }
        }
    }
    

    //[ObserversRpc(RunLocally =false)]
    public void MainBodyDestroyIfDeadObservers()
    {
        root.inputType = PredictionMotor.InputType.Disabled;

        root.gameObject.SetActive(false);

        ChangeCamera();
        base.DestroyIfDeadObservers();
    }

    void OnDestroy()
    {
        if (GetComponentInChildren<Camera>() != null)
        {
            root?.activeIdleCam?.SetEnabled(true);
        }
    }
    

    CameraDampener GetCamInChildren(Transform parent)
    {

        if (parent.TryGetComponent<CameraDampener>(out CameraDampener cam))
            return cam;
        else
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (GetCamInChildren(parent.GetChild(i)) != null)
                {
                    Debug.Log("found " + parent.GetChild(i));
                    return GetCamInChildren(parent.GetChild(i));
                }
            }
            return null;
            //return cam;
        }
    }
   

}
