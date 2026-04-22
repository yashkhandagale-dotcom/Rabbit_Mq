using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        private readonly IConnection _connection;

        public OrderController(IConnection connection)
        {
            _connection = connection;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] string orderName)
        {
            if (string.IsNullOrWhiteSpace(orderName))
                return BadRequest("Order name cannot be empty.");

            await using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "order_exchange",
                type: ExchangeType.Fanout,
                durable: true
            );

            var message = $"Order Created: {orderName}";
            var body = Encoding.UTF8.GetBytes(message);

            var props = new BasicProperties
            {
                Persistent = true
            };

            await channel.BasicPublishAsync(
                exchange: "order_exchange",
                routingKey: "",
                mandatory: false,
                basicProperties: props,
                body: body
            );

            Console.WriteLine($"[OrderService] Published: {message}");
            return Ok(new { status = "Order sent!", order = orderName });
        }
    }
}