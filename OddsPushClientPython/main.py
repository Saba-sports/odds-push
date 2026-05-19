import pika
import sys
import os
import json
from urllib.parse import urlparse

def main():
    # Configuration based on OddsPushClientDotnet/appsettings.json
    connection_url = "changeme"
    exchange_name = "changeme"
    queue_name = "odds-push-client-queue-please-change-name-python" # Using a slightly different name for the demo

    print(f"Connecting to {connection_url}...")

    try:
        parameters = pika.URLParameters(connection_url)
        connection = pika.BlockingConnection(parameters)
        channel = connection.channel()

        # Declare the queue (matching .NET flags: durable=False, auto_delete=True, exclusive=True)
        channel.queue_declare(queue=queue_name, durable=False, exclusive=True, auto_delete=True)

        # Bind the queue to the exchange with routing key "#" (all messages)
        print(f"Binding queue {queue_name} to exchange {exchange_name}...")
        channel.queue_bind(exchange=exchange_name, queue=queue_name, routing_key="#")

        def callback(ch, method, properties, body):
            print("\n--- RAW JSON RECEIVED ---")
            try:
                # Try to pretty print if it's JSON
                data = json.loads(body)
                print(json.dumps(data, indent=2, ensure_ascii=False))
            except json.JSONDecodeError:
                # Otherwise just print raw body
                print(body.decode('utf-8'))
            print("-" * 30)

        channel.basic_consume(queue=queue_name, on_message_callback=callback, auto_ack=True)

        print(f" [*] Waiting for messages from {exchange_name}. To exit press CTRL+C")
        channel.start_consuming()

    except KeyboardInterrupt:
        print("\nInterrupted by user")
        try:
            sys.exit(0)
        except SystemExit:
            os._exit(0)
    except Exception as e:
        print(f"Error: {e}")

if __name__ == '__main__':
    main()
