use futures_util::stream::StreamExt;
use lapin::{
    options::*, types::FieldTable, Connection,
    ConnectionProperties,
};
use serde_json::Value;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let connection_url = "changeme";
    let exchange_name = "changeme";
    let queue_name = "odds-push-client-queue-please-change-name-rust";

    println!("Connecting to {}...", connection_url);

    let conn = Connection::connect(
        connection_url,
        ConnectionProperties::default(),
    )
    .await?;

    println!("Connected");

    let channel = conn.create_channel().await?;

    // Declare Queue
    channel
        .queue_declare(
            queue_name,
            QueueDeclareOptions {
                durable: false,
                exclusive: true,
                auto_delete: true,
                ..QueueDeclareOptions::default()
            },
            FieldTable::default(),
        )
        .await?;

    // Bind Queue
    println!("Binding queue {} to exchange {}...", queue_name, exchange_name);
    channel
        .queue_bind(
            queue_name,
            exchange_name,
            "#",
            QueueBindOptions::default(),
            FieldTable::default(),
        )
        .await?;

    let mut consumer = channel
        .basic_consume(
            queue_name,
            "my_consumer",
            BasicConsumeOptions {
                no_ack: true,
                ..BasicConsumeOptions::default()
            },
            FieldTable::default(),
        )
        .await?;

    println!(" [*] Waiting for messages. To exit press CTRL+C");

    while let Some(delivery) = consumer.next().await {
        let delivery = delivery.expect("error in consumer");
        let data = String::from_utf8_lossy(&delivery.data);

        println!("\n--- RAW JSON RECEIVED ---");
        if let Ok(json) = serde_json::from_str::<Value>(&data) {
            println!("{}", serde_json::to_string_pretty(&json)?);
        } else {
            println!("{}", data);
        }
        println!("------------------------------");
    }

    Ok(())
}
