using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

var factory = new ConnectionFactory()
{
    HostName = "localhost",
    UserName = "email_user",
    Password = "email123",
    VirtualHost = "prod_vhost"
};

var connection = factory.CreateConnection();
var channel = connection.CreateModel();

channel.ExchangeDeclare("order_exchange", ExchangeType.Fanout);

var queueName = channel.QueueDeclare().QueueName;

channel.QueueBind(queueName, "order_exchange", "");

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var message = Encoding.UTF8.GetString(ea.Body.ToArray());
    Console.WriteLine($"📧 Email Sent for: {message}");
};

channel.BasicConsume(queueName, true, consumer);

Console.WriteLine("Waiting for messages...");
Console.ReadLine();