//----------------------------------------------
//            积木2: xiongsonglin
// Copyright © 2015 for Open
//----------------------------------------------
using UnityEngine;
using System.Collections;

public class TransformGameObj : MonoBehaviour {

    int port = 10000;

    private Quaternion q1;
    private Quaternion q2;

    void Test()
    {
        switch (Network.peerType)
        { 
            case NetworkPeerType.Disconnected:
                break;
            case NetworkPeerType.Client:
                break;
            case NetworkPeerType.Connecting:
                break;
            case NetworkPeerType.Server:
                break;
        }
    }
}
