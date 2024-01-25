using System.Net;
using System.Net.Sockets;
using System.Text;

public class Client
{
    Socket mainSock;
    int m_port = 12345;
    public void Connect()
    {
        mainSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPAddress serverAddr = IPAddress.Parse("127.0.0.1");
        IPEndPoint clientEP = new IPEndPoint(serverAddr, m_port);
        mainSock.BeginConnect(clientEP, new AsyncCallback(ConnectCallback), mainSock);
    }
    public void Close()
    {
        if (mainSock != null)
        {
            mainSock.Close();
            mainSock.Dispose();
        }
    }
    public class AsyncObject
    {
        public byte[] Buffer;
        public Socket WorkingSocket;
        public readonly int BufferSize;
        public AsyncObject(int bufferSize)
        {
            BufferSize = bufferSize;
            Buffer = new byte[(long)BufferSize];
        }

        public void ClearBuffer()
        {
            Array.Clear(Buffer, 0, BufferSize);
        }
    }
    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            client.EndConnect(ar);
            AsyncObject obj = new AsyncObject(4096);
            obj.WorkingSocket = mainSock;
            mainSock.BeginReceive(obj.Buffer, 0, obj.BufferSize, 0, DataReceived, obj);
        }
        catch (Exception e)
        {
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
        mainSock.Send(msg);
    }

    public static void Main(string[] args)
    {
        Client client = new Client();
        client.Connect();

        while (true)
        {
            string input = Console.ReadLine();

            if (input == "q")
                break;

            byte[] arr = Encoding.UTF8.GetBytes(input);
            client.Send(arr);
        }
        client.Close();
    }

}
