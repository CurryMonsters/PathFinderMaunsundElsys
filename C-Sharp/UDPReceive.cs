using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;

public class UDPReceive : MonoBehaviour
{

    // Tråd for å håndtere  data
    Thread receiveThread;
    // UDP-klient for å motta data
    UdpClient client;
    // Port som brukes for å lytte etter data
    public int port = 5052;

    public bool startRecieving = true;
    
    public bool printToConsole = false;
    // Variabel for å lagre mottatt data som en tekststreng
    public string data;

    // Array for å lagre koordineringsverdier
    public string[] cords;

    public void Start()
    {
        // Starter en ny thread som håndterer mottak av UDP-data
        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true; 
        receiveThread.Start(); // Start thread
    }

    // Funksjon som håndterer mottak av data på en thread
    private void ReceiveData()
    {
        // Opprett en UDP-klient som lytter på spesifisert port
        client = new UdpClient(port);

        while (startRecieving)
        {
            
            try
            {
                // Lytt etter data fra hvilken som helst IP-adresse
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                // Mottar data som byte-array
                byte[] dataByte = client.Receive(ref anyIP);
                // Konverter byte-array til tekst
                data = Encoding.UTF8.GetString(dataByte);
                // Teksten kommer i en rar format så vil rense teksten, slik at vi bare sitter igjen med verdiene.
                string cleanedData = data.Replace("[", "")
                                          .Replace("]", "")
                                          .Replace("np.float64", "")
                                          .Replace("(", "")
                                          .Replace(")", "");
                // Deler den rensede teksten i separate verdier og lagre i array
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
