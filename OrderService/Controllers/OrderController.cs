using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateOrder([FromBody] string orderName)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "order_user",
                Password = "order123",
                VirtualHost = "prod_vhost"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare("order_exchange", ExchangeType.Fanout);

            var message = $"Order Created: {orderName}";
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                exchange: "order_exchange",
                routingKey: "",
                body: body
            );

            return Ok("Order Sent!");
        }
    }
}