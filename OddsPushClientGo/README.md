# OddsPushClient Go Demo

這是一個簡單的 Go 程式，用來示範如何從與 OddsPushClientDotnet 相同的 RabbitMQ 環境中消費訊息。

## 快速開始

1. 下載依賴：
   ```bash
   go mod tidy
   ```

2. 執行程式：
   ```bash
   go run main.go
   ```

## 功能說明
- 連接到 RabbitMQ 伺服器
- 自動建立一個名為 `odds-push-client-queue-please-change-name-go` 的臨時 Queue (Auto-Delete)
- 綁定到 Exchange `changeme`
- 收到的訊息會以 Raw JSON 格式打印在 Console
