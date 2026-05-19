const amqp = require('amqplib');

async function main() {
  // Configuration based on OddsPushClientDotnet/appsettings.json
  const connectionUrl = "changeme";
  const exchangeName = "changeme";
  const queueName = "odds-push-client-queue-please-change-name-nodejs";

  console.log(`Connecting to ${connectionUrl}...`);

  try {
    const connection = await amqp.connect(connectionUrl);
    const channel = await connection.createChannel();

    // Declare the queue (.NET flags: durable: false, autoDelete: true, exclusive: true)
    await channel.assertQueue(queueName, {
      durable: false,
      exclusive: true,
      autoDelete: true
    });

    // Bind the queue to the exchange with routing key "#"
    console.log(`Binding queue ${queueName} to exchange ${exchangeName}...`);
    await channel.bindQueue(queueName, exchangeName, "#");

    console.log(` [*] Waiting for messages in ${queueName}. To exit press CTRL+C`);

    channel.consume(queueName, (msg) => {
      if (msg !== null) {
        const content = msg.content.toString();
        console.log("\n--- RAW JSON RECEIVED ---");
        try {
          const data = JSON.parse(content);
          console.log(JSON.stringify(data, null, 2));
        } catch (e) {
          console.log(content);
        }
        console.log("-".repeat(30));

        // autoAck is not set here manually in channel.consume options as a separate field usually,
        // but we can pass {noAck: true} to match the autoAck behavior of the other samples.
      }
    }, { noAck: true });

  } catch (error) {
    console.error("Error:", error.message);
    process.exit(1);
  }
}

main();
