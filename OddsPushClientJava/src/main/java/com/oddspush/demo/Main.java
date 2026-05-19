package com.oddspush.demo;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.rabbitmq.client.*;

import java.nio.charset.StandardCharsets;

public class Main {
    public static void main(String[] args) throws Exception {
        // Configuration based on OddsPushClientDotnet/appsettings.json
        String connectionUrl = "changeme";
        String exchangeName = "changeme";
        String queueName = "odds-push-client-queue-please-change-name-java";

        System.out.println("Connecting to " + connectionUrl + "...");

        ConnectionFactory factory = new ConnectionFactory();
        factory.setUri(connectionUrl);

        try (Connection connection = factory.newConnection();
             Channel channel = connection.createChannel()) {

            // Declare Queue: durable: false, autoDelete: true, exclusive: true
            channel.queueDeclare(queueName, false, true, true, null);

            // Bind Queue
            System.out.println("Binding queue " + queueName + " to exchange " + exchangeName + "...");
            channel.queueBind(queueName, exchangeName, "#");

            System.out.println(" [*] Waiting for messages. To exit press CTRL+C");

            Gson gson = new GsonBuilder().setPrettyPrinting().create();

            DeliverCallback deliverCallback = (consumerTag, delivery) -> {
                String message = new String(delivery.getBody(), StandardCharsets.UTF_8);
                System.out.println("\n--- RAW JSON RECEIVED ---");
                try {
                    JsonElement el = gson.fromJson(message, JsonElement.class);
                    System.out.println(gson.toJson(el));
                } catch (Exception e) {
                    System.out.println(message);
                }
                System.out.println("------------------------------");
            };

            channel.basicConsume(queueName, true, deliverCallback, consumerTag -> {});

            // Keep the application running
            while (true) {
                Thread.sleep(1000);
            }
        }
    }
}
