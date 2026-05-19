# OddsPushClient Java Demo

這是一個簡單的 Java 程式，用來示範如何從與 OddsPushClientDotnet 相同的 RabbitMQ 環境中消費訊息。

## 快速開始

1. 編譯與執行：
   ```bash
   mvn clean compile exec:java
   ```

## 功能說明
- 使用 `amqp-client` 連接到 RabbitMQ 伺服器
- 使用 `Gson` 進行 JSON 格式化
- 自動建立一個名為 `odds-push-client-queue-please-change-name-java` 的臨時 Queue (Auto-Delete)
- 綁定到 Exchange `changeme`
- 收到的訊息會以 Raw JSON 格式打印在 Console
