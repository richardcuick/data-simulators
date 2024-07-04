using MQTTnet;
using MQTTnet.Client;

class Program
{
    static async Task Main(string[] args)
    {
        // HiveMQ Broker 信息
        string brokerAddress = "127.0.0.1";
        int brokerPort = 1883;
        string topic = "test/topic";
        string payload = "Hello, HiveMQ!";

        // 创建 MQTT 客户端
        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();

        // 配置客户端选项
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerAddress, brokerPort)
            .WithCleanSession()
            .Build();

        // 连接到 MQTT 服务器
        //mqttClient.UseConnectedHandler(async e =>
        //{

        //});

        //mqttClient.UseDisconnectedHandler(e =>
        //{
        //    Console.WriteLine("Disconnected from HiveMQ Broker.");
        //});

        // 连接并发布
        await mqttClient.ConnectAsync(options);

        Console.WriteLine("Connected to HiveMQ Broker.");

        // 发布消息
        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .WithRetainFlag(false)
            .Build();

        await mqttClient.PublishAsync(message);
        Console.WriteLine($"Published message to topic '{topic}': {payload}");

        // 等待一段时间让消息发布完成
        await Task.Delay(1000);

        // 断开连接
        await mqttClient.DisconnectAsync();
    }
}
