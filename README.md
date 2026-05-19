# OddsPush Client Sample Project

Welcome to the OddsPush product demonstration and integration project. This suite of example clients provides developers with practical implementations for connecting to the OddsPush real-time sports data platform using various programming languages.

## Overview

OddsPush is a high-performance, real-time sports data delivery platform. It utilizes the RabbitMQ message broker to push millisecond-latency updates for events, scores, and odds across a wide range of sports.

### Key Capabilities
- **Real-time Push**: Sub-second data delivery via AMQP protocol.
- **Hierarchical Routing**: Flexible subscription to specific sports or matches via Routing Keys.
- **High Availability**: Built-in heartbeat monitoring and recovery mechanisms.
- **Multi-sport Coverage**: Soccer, Basketball, Tennis, Cricket, and more.

## Project Structure

This repository contains client implementation examples in the following languages:

- [**OddsPushClientDotnet/**](https://github.com/Saba-sports/odds-push/tree/master/OddsPushClientDotnet): C# (.NET 9) implementation (includes a React frontend demo).
- [**OddsPushClientGo/**](https://github.com/Saba-sports/odds-push/tree/master/OddsPushClientGo): Golang reference implementation.
- [**OddsPushClientJava/**](https://github.com/Saba-sports/odds-push/tree/master/OddsPushClientJava): Java (Spring Boot) implementation.
- [**OddsPushClientNodeJS/**](https://github.com/Saba-sports/odds-push/tree/master/OddsPushClientNodeJS): Node.js (TypeScript) implementation.
- [**OddsPushClientPython/**](https://github.com/Saba-sports/odds-push/tree/master/OddsPushClientPython): Python implementation.
- [**OddsPushClientRust/**](https://github.com/Saba-sports/odds-push/tree/master/OddsPushClientRust): Rust high-performance implementation.

## Technical Documentation
- [**English Integration Specification**](https://github.com/Saba-sports/odds-push/wiki/English-Document)
- [**中文整合規格說明**](https://github.com/Saba-sports/odds-push/wiki/%E7%B0%A1%E4%BD%93%E4%B8%AD%E6%96%87%E6%96%87%E6%AA%94)
