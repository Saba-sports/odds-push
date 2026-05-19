package main

import (
	"encoding/json"
	"fmt"
	"log"
	"os"
	"os/signal"
	"strings"
	"syscall"

	amqp "github.com/rabbitmq/amqp091-go"
)

func main() {
	connectionURL := "changeme"
	exchangeName := "changeme"
	queueName := "odds-push-client-queue-please-change-name-go"

	log.Printf("Connecting to %s...", connectionURL)
	conn, err := amqp.Dial(connectionURL)
	if err != nil {
		log.Fatalf("Failed to connect to RabbitMQ: %v", err)
	}
	defer conn.Close()

	ch, err := conn.Channel()
	if err != nil {
		log.Fatalf("Failed to open a channel: %v", err)
	}
	defer ch.Close()

	// Declare the queue
	// .NET flags: durable: false, autoDelete: true, exclusive: true
	q, err := ch.QueueDeclare(
		queueName, // name
		false,     // durable
		true,      // delete when unused
		true,      // exclusive
		false,     // no-wait
		nil,       // arguments
	)
	if err != nil {
		log.Fatalf("Failed to declare a queue: %v", err)
	}

	// Bind the queue to the exchange with routing key "#"
	log.Printf("Binding queue %s to exchange %s...", q.Name, exchangeName)
	err = ch.QueueBind(
		q.Name,       // queue name
		"#",          // routing key
		exchangeName, // exchange
		false,
		nil,
	)
	if err != nil {
		log.Fatalf("Failed to bind a queue: %v", err)
	}

	msgs, err := ch.Consume(
		q.Name, // queue
		"",     // consumer
		true,   // auto-ack
		false,  // exclusive
		false,  // no-local
		false,  // no-wait
		nil,    // args
	)
	if err != nil {
		log.Fatalf("Failed to register a consumer: %v", err)
	}

	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, syscall.SIGINT, syscall.SIGTERM)

	log.Printf(" [*] Waiting for messages. To exit press CTRL+C")

	go func() {
		for d := range msgs {
			fmt.Println("\n--- RAW JSON RECEIVED ---")
			var prettyJSON interface{}
			err := json.Unmarshal(d.Body, &prettyJSON)
			if err != nil {
				// If not JSON, print raw string
				fmt.Printf("%s\n", d.Body)
			} else {
				formattedJSON, _ := json.MarshalIndent(prettyJSON, "", "  ")
				fmt.Printf("%s\n", string(formattedJSON))
			}
			fmt.Println("------------------------------")
		}
	}()

	<-sigChan
	log.Printf("Shutting down...")
}
