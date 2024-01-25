using System.Net;
using System.Net.Sockets;
using System.Text;

public class Server
{
    // serverside socket, port
    Socket       mainsock;
    int          m_port= 12345;

    // client list
    List<Socket> connectedClients = new List<Socket>();

    public void Start()
    {
        try 
        {
            // socket create
            mainsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // set EndPoint
            IPEndPoint serverEP = new IPEndPoint(IPAddress.Any, m_port);
            // Bind Socket/EndPoint
            mainsock.Bind(serverEP);
            // Listen 
            mainsock.Listen(10);

            Console.WriteLine("mainsock Start");

            mainsock.BeginAccept(AcceptCallback, null);
        }
        catch (Exception e)
        {
            Console.WriteLine("mainsock err");
        }
    }

    public void Close()
    {
        // disconnect mainsock
        if (mainsock != null)
        {
            mainsock.Close();
            mainsock.Dispose();
        }

        // disconnect client sock
        foreach (Socket socket in connectedClients)
        {
            socket.Close();
            socket.Dispose();
        }
        connectedClients.Clear();
        Console.WriteLine("Close");
    }

    public class AsyncObject
    {
        public byte[]       Buffer;
        public Socket       WorkingSocket;
        public readonly int BufferSize;

        // bufferSize가 주어진 AsyncObject 생성자 
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[(long)BufferSize];
        }
    }

    // mainsock receive connect
    void AcceptCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = mainsock.EndAccept(ar);
            // buf obj 생성
            AsyncObject obj = new AsyncObject(4096);
            // obj client 지정
            obj.WorkingSocket = client;
            // client list 추가
            connectedClients.Add(client);
            // datareceived 콜백
            client.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);

            // accept 콜백
            mainsock.BeginAccept(AcceptCallback, null);

            Console.WriteLine("AcceptCallback");
        }
        catch (Exception e)
        {
            Console.WriteLine("AcceptCallback err");
        }
    }

    void DataReceived(IAsyncResult ar)
    {
        AsyncObject obj = (AsyncObject)ar.AsyncState;

        try
        {
            int received = obj.WorkingSocket.EndReceive(ar);

            if (received > 0)
            {
                byte[] buffer = new byte[received];
                Array.Copy(obj.Buffer, 0, buffer, 0, received);

                // 받은 데이터를 콘솔에 출력
                string receivedData = Encoding.UTF8.GetString(buffer);
                Console.WriteLine("받은 데이터: " + receivedData);

                // connectedClients에 있는 모든 클라이언트에게 데이터 전송
                foreach (Socket client in connectedClients)
                {
                    try
                    {
                        // 클라이언트에 데이터 전송
                        client.Send(buffer);
                    }
                    catch (Exception e){}
                }
            }

            // 다시 데이터 수신 대기
            obj.WorkingSocket.BeginReceive(obj.Buffer, 0, 4096, 0, DataReceived, obj);
        }
        catch (Exception e)
        {
            Console.WriteLine("DataReceived 에러: " + e.Message);
        }
    }

    public void Send(byte[] msg)
    {
        mainsock.Send(msg);
    }

    public static void Main()
    {
        Server server = new Server();
        server.Start();

        while(true) 
        {
            string input = Console.ReadLine();

            if (input == "q")
                break;
            
            byte[] arr = Encoding.UTF8.GetBytes(input);
            server.Send(arr);
        }
        server.Close();
    }
}