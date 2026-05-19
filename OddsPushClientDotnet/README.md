# OddsPush Sportsbook Demo (.NET + React)

This project is a sports betting demonstration application built with .NET 9 Web API and React. It consumes real-time match data from RabbitMQ, filters for **Basketball (SportType: 2) and Soccer (SportType: 1)**, persists the data to SQLite, and displays it through a modern frontend interface.

## Key Features
- **Real-time Data Consumption**: Automatically receives message types 0 (Event), 1 (EventState), and 2 (Market).
- **Data Filtering**: Processes and stores only events where `sportType = 1 (Soccer)` or `2 (Basketball)`.
- **Auto-Maintenance Mode**: If no MessageType 3 (Heartbeat) is received for over 10 minutes, the API will automatically return a `503 Service Unavailable`.
- **Automatic Data Cleanup**: Cleans up closed events and markets (status: `closed`) that are older than 5 minutes to keep the database lightweight.
- **Persistence**: Powered by Entity Framework Core + SQLite.
- **Modern UI**: Built with React + Vite + Tailwind CSS.

## API Specification

### 1. Get Event List
`GET /api/Events`

- **Query Parameters**:
  - `sportType` (int, optional): Sport type code.
  - `eventStatus` (string, optional): Match status (e.g., `not_started`, `live`, `closed`).
- **Response**: `200 OK` (Array of EventDto) / `503 Service Unavailable` (Heartbeat expired).

### 2. Get Single Event Details
`GET /api/Events/{id}`

- **Path Parameters**:
  - `id` (long, required): Unique Event ID.
- **Response**: `200 OK` (EventDto) / `404 Not Found` / `503 Service Unavailable`.

### Data Model (EventDto)
| Field | Type | Description |
| :--- | :--- | :--- |
| `eventId` | long | Match ID |
| `homeTeamName` | string | Home Team Name |
| `awayTeamName` | string | Away Team Name |
| `kickoffTime` | dateTime | Kickoff Time |
| `isLive` | bool | Is In-play/Live match |
| `liveHomeScore` | int | Home Team Current Score |
| `markets` | list | List of markets (includes localized bet type names) |

## Quick Start

### 1. Backend Configuration (.NET)
Navigate to the root directory and update `appsettings.json`:
```json
"OddsPush": {
  "Connection": "Your RabbitMQ Connection String",
  "ExchangeName": "Your Exchange Name"
}
```

### 2. Run Backend
```bash
cd OddsPushClientDotnet
dotnet run
```
- API Path: `http://localhost:5078` (subject to `launchSettings.json`)
- Swagger: `http://localhost:5078/swagger`

### 3. Run Frontend (React)
```bash
cd OddsPushClientDotnet/sportsbook-ui
npm install
npm run dev
```
- Frontend Path: `http://localhost:5173` (subject to Vite output)

## System Monitoring & Data Maintenance
- **Heartbeat Circuit Breaker**: If RabbitMQ disconnects or the server stops sending heartbeats for 10 minutes, the frontend will receive a 503 error, which can be used to display a maintenance notification.
- **Data Cleanup**: To maintain performance, the system triggers a check upon receiving heartbeats. Records with `eventStatus` or `marketStatus` of `closed` for more than 5 minutes are permanently deleted.
  - *Recommendation*: In production, evaluate moving closed data to historical storage (Cold DB) instead of immediate deletion.

## ⚠️ Production Disclaimer
**Important:** This project is for technical demonstration and Proof of Concept (PoC) only. It is intended to show how to integrate with the OddsPush platform and is not designed for high availability or commercial operations. 

**Do NOT deploy this project directly to production.** Implement robust security, error handling, database clustering, and scaling before going live.

---

# OddsPush Sportsbook Demo (.NET + React) (中文)

這是一個基於 .NET 9 Web API 與 React 的運動博彩展示專案。本專案會從 RabbitMQ 接收即時賽事資料，過濾出**籃球 (SportType: 2) 與 足球 (SportType: 1)** 資料，持久化至 SQLite，並透過現代化前端介面展示。

## 核心功能
- **即時數據消費**: 自動接收 MessageType 0 (Event), 1 (EventState), 2 (Market)。
- **資料過濾**: 僅處理並儲存 `sportType = 1 (足球) or 2 (籃球)` 的賽事。
- **自動維護模式**: 若超過 10 分鐘未收到 MessageType 3 (Heartbeat)，API 將自動回傳 503 服務不可用。
- **資料自動清理**: 系統會自動清理已關閉 (`status: closed`) 超過 5 分鐘的賽事與盤口資料，確保資料庫輕量化。
- **持久化**: 使用 Entity Framework Core + SQLite。
- **現代化 UI**: React + Vite + Tailwind CSS。

## API 規格 (API Specification)

### 1. 取得賽事列表
`GET /api/Events`

- **Query Parameters**:
  - `sportType` (int, optional): 運動類型。
  - `eventStatus` (string, optional): 賽事狀態 (如: `not_started`, `live`, `closed`)。
- **Response**: `200 OK` (Array of EventDto) / `503 Service Unavailable` (心跳逾期)。

### 2. 取得單一賽事詳情
`GET /api/Events/{id}`

- **Path Parameters**:
  - `id` (long, required): 賽事唯一識別碼 (EventID)。
- **Response**: `200 OK` (EventDto) / `404 Not Found` / `503 Service Unavailable`。

## 快速啟動

### 1. 後端配置 (.NET)
進入專案根目錄，更新 `appsettings.json`：
```json
"OddsPush": {
  "Connection": "您的 RabbitMQ 連線字串",
  "ExchangeName": "您的 Exchange 名稱"
}
```

### 2. 啟動後端
```bash
cd OddsPushClientDotnet
dotnet run
```
- API 地址: `http://localhost:5078`
- Swagger 文檔: `http://localhost:5078/swagger`

### 3. 啟動前端 (React)
```bash
cd OddsPushClientDotnet/sportsbook-ui
npm install
npm run dev
```

## 系統監控與資料維護
- 此系統具備「心跳斷路器」機制。若 RabbitMQ 連線中斷或 server 停止發送心跳包超過 10 分鐘，前端獲取資料時會收到 503 錯誤。
- **資料清理機制**: 任何 `eventStatus` 或 `marketStatus` 為 `closed` 且持續超過 5 分鐘的記錄將被永久刪除。

## ⚠️ 生產環境警告 (Disclaimer)
**重要提示：** 本專案僅供技術展示及開發概念驗證 (PoC) 之用。**嚴禁直接將此專案部署至生產環境 (Production)。**

---
🤖 *Generated as a Technical Demo*
