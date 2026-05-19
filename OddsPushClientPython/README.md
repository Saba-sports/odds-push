# OddsPushClient Python Demo

這是一個簡單的 Python 程式，用來示範如何從與 OddsPushClientDotnet 相同的 RabbitMQ 環境中消費訊息。

## 快速開始

1. 安裝套件：
   ```bash
   pip install -r requirements.txt
   ```

2. 執行程式：
   ```bash
   python main.py
   ```

## 功能說明
- 連接到 RabbitMQ 伺服器
- 自動建立一個名為 `odds-push-client-queue-please-change-name-python` 的臨時 Queue (Auto-Delete)
- 綁定到 Exchange `changeme`
- 收到的訊息會以 Raw JSON 格式打印在 Console
