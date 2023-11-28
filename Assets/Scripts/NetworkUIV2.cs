using FishNet.Managing;
using FishNet.Transporting.UTP;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;
using TMPro;
using UnityEditor;

public class NetworkUIV2 : MonoBehaviour
{
    [SerializeField] private NetworkManager _networkManager;

    string joinCode = "";
    FishyUnityTransport utp;

    bool serverStarted = false;
    public bool clientStarted { get; private set; } = false;

    [SerializeField]
    TMP_Text serverButtonText;

    [SerializeField]
    TMP_Text clientButtonText;

    [SerializeField]
    TMP_InputField joinCodeBox;

    public InputManager inputManager;

    [SerializeField]
    GameObject optionsMenu;

    
    private async void Start()
    {
        utp = (FishyUnityTransport)_networkManager.TransportManager.Transport;
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
            {
              Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateRelay()
    {
        //clientButtonText.text = "Starting\nClient";

        if (!serverStarted)
        {
            serverButtonText.text = "Starting\nServer";
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(20);

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                utp.SetRelayServerData(new RelayServerData(allocation, "dtls"));

                Debug.Log(joinCode);
                joinCodeBox.text = joinCode;
                joinCodeBox.interactable = false;
                this.joinCode = joinCode;
                // Start Server Connection
                _networkManager.ServerManager.StartConnection();
                // Start Client Connection
                JoinRelay();

                serverButtonText.text = "Stop\nServer";
                //clientButtonText.text = "Stop\nClient";
                serverStarted = true;

               
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                StartCoroutine(ShowServerFailedToStartErrorText());
                
                serverStarted = false;
            }



        }
        else
        {
            _networkManager.ServerManager.StopConnection(true);

            serverButtonText.text = "Start\nServer";
            JoinRelay();

           
            joinCodeBox.interactable = true;
            serverStarted = false;

        }


       /* if (serverStarted && !inputManager.menuUp)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

        }
        if (clientStarted && serverStarted)

            ToggleNetUIVisability(optionsMenu.activeInHierarchy);*/
    }

    public async void JoinRelay()
    {

        if (!clientStarted)
        {
            clientButtonText.text = "Starting\nClient";

            if (joinCode.Length != 6)
            {
                Debug.LogWarning("No joinCode provided, or code is invalid! Code:" + joinCode);
                StartCoroutine(ShowJoinCodeErrorText());
                return;
            }

            try
            {
                Debug.Log("Joining Relay with " + joinCode);
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

                if (!serverStarted)
                {

                    utp.SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
                }
                _networkManager.ClientManager.StartConnection();
                clientButtonText.text = "Stop\nClient";

                clientStarted = true;

                if (!inputManager.menuUp)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
            catch (RelayServiceException e)
            {
                Debug.LogError(e);
                StartCoroutine(ShowJoinCodeErrorText());
                clientStarted = false;
            }



        }
        else
        {
            _networkManager.ClientManager.StopConnection();
            clientButtonText.text = "Start\nClient";
            clientStarted = false;
        }

        if(clientStarted)
        ToggleNetUIVisability(optionsMenu.activeInHierarchy);

    }

    public void SyncJoinCode(string s)
    {
        joinCode = s;
        
    }

    [SerializeField]
    GameObject networkButtons;

    public void ToggleNetUIVisability(bool setActive)
    {
        networkButtons.SetActive(setActive);
    }

    IEnumerator ShowJoinCodeErrorText()
    {
        clientButtonText.text = "Invalid\nCode!";
        yield return new WaitForSeconds(2f);
        clientButtonText.text = "Start\nClient";
    }
    IEnumerator ShowServerFailedToStartErrorText()
    {
        clientButtonText.text = "Server Failed\nto Start!";
        yield return new WaitForSeconds(2f);
        clientButtonText.text = "Start\nServer";
    }

}