using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

public class UDPReceive : MonoBehaviour
{

    Thread receiveThread;
    UdpClient client;
    public int port = 5052;
    public bool startRecieving = true;
    public bool printToConsole = false;
    public string data;

    public string[] cords;

    public void Start()
    {

        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }


    // receive thread
    private void ReceiveData()
    {

        client = new UdpClient(port);
        while (startRecieving)
        {

            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] dataByte = client.Receive(ref anyIP);
                data = Encoding.UTF8.GetString(dataByte);
                string cleanedData = data.Replace("[", "")
                                          .Replace("]", "")
                                          .Replace("np.float64","")
                                          .Replace("(","")
                                          .Replace(")","");
                cords = cleanedData.Split(',');

                if (printToConsole) { print(cords); }
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

}
