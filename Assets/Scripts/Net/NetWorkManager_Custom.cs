using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NetWorkManager_Custom : NetworkManager {

    [SerializeField] GameObject LoadingObject;

    public void StartupHost()
    {
        if (NetworkClient.active) return;
        SetPort();
        SetLoading(true);
        NetworkManager.singleton.StartHost();   
    }

    public void SetLoading(bool state)
    {
        GameObject.FindGameObjectWithTag("Canvas").GetComponent<Canvas>().enabled = !state;
        LoadingObject.SetActive(state);
    }

    public void JoinGame()
    {
        if (NetworkClient.active) return;
        SetIPAddress();
        SetPort();
        var adress = GameObject.Find("InputFieldIPAddress").GetComponent<TMP_InputField>().text;
        if (adress == "") return;
        singleton.networkAddress = adress;
        SetLoading(true);
        NetworkManager.singleton.StartClient();
    }

    void SetIPAddress()
    {
        string ipAdress = GameObject.Find("InputFieldIPAddress").GetComponent<TMP_InputField>().text;
        NetworkManager.singleton.networkAddress = ipAdress;
    }

    public void JoinLocalGame()
    {
        if (NetworkClient.active) return;
        networkAddress = "localhost";
        //singleton.networkAddress = "localhost";
        //singleton.networkPort = 7777;
        //singleton.StartClient();
        SetLoading(true);
        StartClient();
    }

/*    public void UnSpawnCutom(GameObject destroingObject)
    {
        destroingObject.tag = "";
        NetworkServer.Destroy(destroingObject);
    }*/

    void SetPort()
    {
        //NetworkManager.singleton.networkPort = 7777;
    }

    public void StopHostOwn()
    {
        StopHost();
        StopClient();
        //SceneManager.LoadScene(0);
    }
}
