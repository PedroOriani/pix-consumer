﻿using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using PixConsumer.DTOs;

var connString = "Host=localhost;Username=postgres;Password=2483;Database=pix";
await using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

string queueName = "payments";
var factory = new ConnectionFactory
{
    HostName = "localhost",
    UserName = "guest",
    Password = "guest"
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: queueName,
    durable: true,
    exclusive: false,
    autoDelete: false,
    arguments: null
);

Console.WriteLine("[*] Waiting for messages...");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);

    PaymentDTO payment = JsonSerializer.Deserialize<PaymentDTO>(message);
    var jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase // Define a política de nomenclatura para minúsculas
    };
    var jsonContent = JsonSerializer.Serialize(payment, jsonOptions);
    Console.WriteLine($"JSON enviado: {jsonContent}");

    using var httpClient = new HttpClient();
    var response = await httpClient.PostAsync("http://localhost:5039/payments/pix", new StringContent(jsonContent, Encoding.UTF8, "application/json"));
    var statusResponse = response.IsSuccessStatusCode ? "SUCCESS" : "FAILED";

    await using (var cmd = new NpgsqlCommand("UPDATE \"Payments\" SET \"Status\" = (@status) WHERE \"Id\" = @id", conn))
    {
        cmd.Parameters.AddWithValue("status", statusResponse);
        cmd.Parameters.AddWithValue("id", payment.Id);
        await cmd.ExecuteNonQueryAsync();
    }

    Console.WriteLine("Payment updated!");
};

channel.BasicConsume(
    queue: queueName,
    autoAck: true,
    consumer: consumer
);

Console.WriteLine("Press [enter] to exit");
Console.ReadLine();