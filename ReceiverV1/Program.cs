using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        // 服务器的IP和端口
        string serverIp = "127.0.0.1";
        int serverPort = 3000;

        // 创建TCP客户端
        using (TcpClient client = new TcpClient())
        {
            try
            {
                // 连接到服务器
                await client.ConnectAsync(serverIp, serverPort);
                Console.WriteLine("Connected to server");

                // 获取网络流
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;

                    while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        // 将字节数组转换为字符串
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"Received: {receivedData}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
