using System;
using BalatroOnline.Network.SocketProtocol;
using UnityEngine;

[Serializable]
public class LoginRequest : BaseSocket
{
    public override string EventName => "LoginRequest";

    public string email;
    public string password;

    public LoginRequest(string email, string password)
    {
        this.email = email;
        this.password = password;
    }
}