using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoBehaviour
{
    public static Client instance;
    public static int dataBufferSize = 4096;


    // public string ip = "20.52.121.149";

   public string ip = "192.168.144.39";
    public int port = 5555;
    public int myId = 0 ;
    public TCP tcp;
    public UDP udp;

    private bool isConnected = false;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    private void Start()
    {
        print("Start");
        tcp = new TCP();
        udp = new UDP();

        print("establish connection");
        Client.instance.ConnectToServer();
    }
    private void OnApplicationQuit() {
        Disconnect();
    }
    
    public void ConnectToServer()
    {
        InitializeClientData();
        isConnected = true;
        tcp.Connect();
    }

    public class TCP
    {
        public TcpClient socket;

        private NetworkStream stream;
        private Packet receivedData;
        private byte[] receiveBuffer;

        public void Connect()
        {
            TcpClient tcpClient = new TcpClient();
            try 
            {
                tcpClient.Connect(instance.ip, instance.port);
                print("Port open");
            } 
            catch (Exception)
            {
                print("Port closed");
                return;
            }

            socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            receiveBuffer = new byte[dataBufferSize];
            socket.BeginConnect(instance.ip, instance.port, ConnectCallback, socket);

        }

        private void ConnectCallback(IAsyncResult _result)
        {
            socket.EndConnect(_result);

            if (!socket.Connected)
            {
                print("server jest wyłączony");
                return;
            }

            stream = socket.GetStream();

            receivedData = new Packet();

            stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
        }

        public void SendData(Packet _packet)
        {
            try
            {
                if (socket != null)
                {
                    stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via TCP: {_ex}");
            }
        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);

                receivedData.Reset(HandleData(_data));
                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] _data)
        {
            int _packetLength = 0;

            receivedData.SetBytes(_data);

            if (receivedData.UnreadLength() >= 4)
            {
                _packetLength = receivedData.ReadInt();
                if (_packetLength <= 0)
                {
                    return true;
                }
            }

            while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
            {
                byte[] _packetBytes = receivedData.ReadBytes(_packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_packetBytes))
                    {
                        int _packetId = _packet.ReadInt();
                        packetHandlers[_packetId](_packet);
                    }
                });

                _packetLength = 0;
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }
            }

            if (_packetLength <= 1)
            {
                return true;
            }

            return false;
        }
        
        private void Disconnect() {
            instance.Disconnect();
            stream = null;
            receivedData = null;
            receivedData = null;
            socket = null;
        }
    }
    public class UDP
    {
        public UdpClient socket;
        public IPEndPoint endPoint;

        public UDP()
        {
            endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);
        }

        public void Connect(int _localPort)
        {
           try{  
                if(endPoint == null) endPoint = new IPEndPoint(IPAddress.Parse(instance.ip), instance.port);

                socket = new UdpClient(_localPort);

                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

             
                using (Packet _packet = new Packet())
                {
                    SendData(_packet);
                }
            }catch(Exception _Ex){
                print("CONNECT UDP -> "+_Ex.Message);
            }

        }

        public void SendData(Packet _packet)
        {
            try
            {
                _packet.InsertInt(instance.myId);
                if (socket != null)
                {
                    socket.BeginSend(_packet.ToArray(), _packet.Length(), null, null);
                }
            }
            catch (Exception _ex)
            {
                Debug.Log($"Error sending data to server via UDP: {_ex}");
                // test ponownego połączenia sie z udp ?
                try
                {
                    Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
                }
                catch (System.Exception ex)
                {
                    print("proba ponownego połączenia sie z udp + "+ex.Message);
                }
            }
        }

        public void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                byte[] _data = socket.EndReceive(_result, ref endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                if (_data.Length < 4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(_data);
            }
            catch
            {
                Disconnect();
            }
        }

        private void HandleData(byte[] _data)
        {
            using (Packet _packet = new Packet(_data))
            {
                int _packetLength = _packet.ReadInt();
                _data = _packet.ReadBytes(_packetLength);
            }

            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (Packet _packet = new Packet(_data))
                {
                    int _packetId = _packet.ReadInt();
                 try
                 {
                    packetHandlers[_packetId](_packet);
                     
                 }
                 catch (System.Exception)
                 {
                     print("jakis blad emmm???");
                    // throw;
                 }   
                }
            });
        }
        private void Disconnect() {
            instance.Disconnect();
            endPoint = null;
            socket = null;
        }
    }
    
    private void InitializeClientData()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, ClientHandle.Welcome },                                       // potwierdzenie poprawnego logowania sie do serwera
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer },                               // pojawienie sie gracza na scenie
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition },                         // odebranie nowej pozycji gracza ktory sie wykonał ruch (w tym local)
            { (int)ServerPackets.updateChat, ClientHandle.UpdateChat },                                 // polecenie aktualizacji czatu o wiadomość GM'a
            { (int)ServerPackets.updateChat_NewUserPost, ClientHandle.UpdateChat_NewUserPost },         // polecenie aktualizacji czatu o post innego uzytkownika
            { (int)ServerPackets.removeOfflinePlayer, ClientHandle.RemoveOfflinePlayer },               // info o osobach ktore sie wylogowały, i ktore mamy usunąć ze sceny
            { (int)ServerPackets.ping_ALL, ClientHandle.PingBackToServer },                             // zwykły heartbeat co 5 sekund czy klient nadal aktywny
            { (int)ServerPackets.downloadMapData, ClientHandle.SendMapToServer },                       // klient ma wysłać swoją mape na serwer
            { (int)ServerPackets.sendCurrentUpdateNumber, ClientHandle.ReceivedUpdateNumber },          // otrzymanie info o nowym Update
            { (int)ServerPackets.SEND_MAPDATA_TO_CLIENT, ClientHandle.NewMapDataFromServerReceived },   // otrzymano nową mape
            { (int)ServerPackets.RegistrationResponse, ClientHandle.RetievedRegistrationResponse },     // informacja zwrotna na wysłanie danych rejestracji konta na serwerze
            { (int)ServerPackets.LoginResponse, ClientHandle.RetievedLoginResponse }                     // informacja zwrotna na dotycząca proby logowania

            // TODO:     // otrzymanie info o szczegółach zebranego przedmiotu
            // TODO:     // otrzymanie szczegółów dotycznących napotkanego NPC'a 
            // TODO:     // otrzymanie info o aktualnych ofertach w sklepie (jeze,lli ktos cos sprzeda to bedzie mozna to odkupic od npc ?)
            // TODO:     // otrzymanie potwierdzenia akceptacji zaproszenia przez innego gracza do party
            // TODO:     // otrzymanie aktualnych informacji dotyczące innego gracza ( level, pozycja, w zakladce "party" jezeli jestescie w grupie )   
        };
        Debug.Log("Initialized packets.");
    }
    
    private void Disconnect() {
            if(isConnected) {
                isConnected = false;
                
                tcp.socket.Close();
                udp.socket.Close();

                Debug.Log("Disconnectef from server.");
            }
            ThreadManager.ExecuteOnMainThread(()=>UIManager.instance.BackToStartScreen());
           UIManager.instance.BackToStartScreen();
        }
}
