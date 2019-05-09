using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class LoggerNetClient
{
    TcpClient socketConnection;
    Thread clientReceiveThread;
    bool isAvailable = true;
    int port = 0;

    public bool IsAvailable()
    {
        return isAvailable;
    }

    public void Send(string msg)
    {
        if (!isAvailable)
        {
            return;
        }

        if (clientReceiveThread == null ||!clientReceiveThread.IsAlive)
        {
            ConnectToServer(port);
        }

        SendMessage(msg);
    }

    
    /// <summary> 	
    /// Setup socket connection. 	
    /// </summary> 	
    public LoggerNetClient(int port)
    {
        this.port = port;

        try
        {
            ConnectToServer(port);
        }
        catch (Exception e)
        {
            isAvailable = false;
            Logger.Error("Logger client initialization exception\n" + e);
        }
    }

    void ConnectToServer(int port)
    {
        try
        {
            socketConnection = new TcpClient("localhost", port);
        }
        catch (SocketException socketException)
        {
            isAvailable = false;
            Logger.Error("Socket exception on connecting : " + socketException);
        }
    }

    /// <summary> 	
    /// Send message to server using socket connection. 	
    /// </summary> 	
    private void SendMessage(string msg)
    {
        if (socketConnection == null)
        {
            return;
        }
        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = socketConnection.GetStream();
            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(msg);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);
            }
        }
        catch (SocketException socketException)
        {
            isAvailable = false;
            Logger.Error("Socket exception on sending : " + socketException);
        }
    }
}