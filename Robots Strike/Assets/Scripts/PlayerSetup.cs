﻿using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
// dziedziczymy z klasy NetworkBehaviour
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    Behaviour[] componentsToDisable;

    [SerializeField]
    string remoteLayerName = "RemotePlayer";

    [SerializeField]
    string dontDrawLayerName = "DontDraw";

    [SerializeField]
    GameObject playerGraphics;

    [SerializeField]
    GameObject playerUIPrefab;

    [HideInInspector]
    public GameObject playerUIInstance;

    private void Start()
    {
        // jeżeli nie jesteśmy lokalnym graczem(tym którym sterujemy) to wyłączmy komponenty 
        // ruchu itd u innych postaci żeby nie było tak, że jak jedna postać się przemieszcza
        // to i druga to robi...
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {   
            // Disable player graphics for Local player(recursive method)
            SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));

            // Create player UI
            playerUIInstance = Instantiate(playerUIPrefab);
            // clean clone
            playerUIInstance.name = playerUIPrefab.name;

            // configure player UI
            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if(ui == null)
            {
                Debug.LogError("No player UI component on PlayerUI prefab!");
            }
            ui.SetPlayer(GetComponent<Player>());

            GetComponent<Player>().SetupPlayer();

            string _username = "Loading...";
            if(UserAccountManager.IsLoggedIn)
            {
                _username = UserAccountManager.LoggedIn_Username;
            }
            else
            {
                _username = transform.name;
            }

            CmdSetUsername(transform.name, _username);
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        // go through all children objects
        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    // whenever someone joins..
    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID, _player);
    }

    // only on server executed
    [Command]
    void CmdSetUsername(string playerID, string username)
    {
        Player player = GameManager.GetPlayer(playerID);
        if(player != null)
        {
            Debug.Log(username + " joined!");
            player.username = username;
        }
    }

    void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }

    void DisableComponents()
    {
        for (int i = 0; i < componentsToDisable.Length; i++)
        {
            componentsToDisable[i].enabled = false;
        }
    }

    private void OnDisable()
    {
        // destroy playerUI
        Destroy(playerUIInstance);

        // jeżeli wychodzimy z gry, włącz kamerę lokalną
        if(isLocalPlayer)
        GameManager.instance.SetSceneCameraActive(true);

        // deregister player on disconnection
        GameManager.UnRegisterPlayer(transform.name);
    }
}
