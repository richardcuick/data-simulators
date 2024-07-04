using OfficeOpenXml;
using System.Net;
using System.Net.Sockets;

class Program
{
    static async Task Main(string[] args)
    {

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        // Excel文件路径
        string excelFilePath = "data.xlsx";

        // TCP服务器监听的端口
        int port = 3000;

        // 客户端列表
        List<TcpClient> clients = new List<TcpClient>();

        // 启动TCP服务器
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}");

        // 接受客户端连接
        _ = Task.Run(async () =>
        {
            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();
                lock (clients)
                {
                    clients.Add(client);
                }
                Console.WriteLine("Client connected");
            }
        });

        // 打开Excel文件
        using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
        {
            // 获取第一个工作表
            var worksheet = package.Workbook.Worksheets[0];

            // 获取行和列的总数
            int rowCount = worksheet.Dimension.Rows;
            int colCount = worksheet.Dimension.Columns;

            string packageHeader = "&&\n\r";
            string packageFooter = "\n\r!!\n\r";
            string delimiter = "\n\r";

            while (true)
            {
                for (int row = 2; row <= rowCount; row++) // 从第2行开始读取数据（假设第1行为标题）
                {
                    // 读取每一行的数据
                    var rowData = new object[colCount];
                    for (int col = 1; col <= colCount; col++)
                    {
                        string key = col.ToString().PadLeft(4, '0');
                        string value = worksheet.Cells[row, col].Text;
                        rowData[col - 1] = string.Concat(key, value);
                    }

                    // 将数据序列化为字符串，按需要的格式（这里假设使用逗号分隔）
                    //string dataToSend = string.Join(",", rowData);
                    //string dataToSend = "&&\n\r" + string.Join("\n\r", rowData) + "!!\n\r";
                    string dataToSend = string.Concat(packageHeader, string.Join(delimiter, rowData), packageFooter);

                    // 将字符串转换为字节数组
                    //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(dataToSend);
                    byte[] buffer = System.Text.Encoding.ASCII.GetBytes(dataToSend);

                    // 发送数据给所有客户端
                    lock (clients)
                    {
                        List<TcpClient> disconnectedClients = new List<TcpClient>();

                        foreach (var client in clients)
                        {
                            if (client.Connected)
                            {
                                try
                                {
                                    NetworkStream stream = client.GetStream();
                                    stream.WriteAsync(buffer, 0, buffer.Length);
                                    Console.WriteLine($"Sent to client: {dataToSend}");
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error sending data to client: {ex.Message}");
                                    disconnectedClients.Add(client);
                                }
                            }
                            else
                            {
                                disconnectedClients.Add(client);
                            }
                        }

                        // 移除已断开的客户端
                        foreach (var disconnectedClient in disconnectedClients)
                        {
                            clients.Remove(disconnectedClient);
                            disconnectedClient.Close();
                        }
                    }

                    // 每秒发送一行数据
                    await Task.Delay(1000);
                }
            }
        }

        // 停止服务器
        listener.Stop();
    }
}
