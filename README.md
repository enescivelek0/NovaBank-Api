# NovaBank-Api 🏦

[![CI/CD Pipeline](https://github.com/enescivelek0/NovaBank-Api/actions/workflows/ci.yml/badge.svg)](https://github.com/enescivelek0/NovaBank-Api/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Live Demo](https://img.shields.io/badge/🌐_Canlı_Demo-novabank--api.onrender.com-6366f1?style=flat)](https://novabank-api-1.onrender.com/index.html)

---

## 🌐 Canlı Demo

> **👉 [https://novabank-api-1.onrender.com/index.html](https://novabank-api-1.onrender.com/index.html)**

Swagger API dokümantasyonu: **[https://novabank-api-1.onrender.com/swagger](https://novabank-api-1.onrender.com/swagger)**

---

**NovaBank-Api**, kurumsal standartlarda **Clean Architecture**, **CQRS (MediatR)**, **Repository & Unit of Work**, **Entity Framework Core**, **JWT Authentication** ve **xUnit / Moq** test altyapısı kullanılarak geliştirilmiş yeni nesil dijital bankacılık ve hesap yönetim REST API sistemidir.

---

## 🏛️ Mimari Katmanlar

```
NovaBank-Api/
├── .github/workflows/         # Otomatik derleme & test (CI/CD) iş akışı
├── src/
│   ├── Banking.Domain/        # Saf Domain modelleri, Value Object'ler, Enum'lar ve Exception'lar
│   ├── Banking.Application/   # CQRS Komut/Sorguları, MediatR, FluentValidation, Pipeline Behaviors
│   ├── Banking.Infrastructure/# EF Core DbContext, Fluent API, Repository, UoW, JWT
│   └── Banking.API/           # REST API Controller'ları, Swagger/OpenAPI, Global Exception Middleware
├── tests/
│   └── Banking.UnitTests/     # xUnit, Moq ve FluentAssertions birim testleri
├── Dockerfile                 # Çok aşamalı (multi-stage) üretim Docker imajı
├── render.yaml                # Render platformu için tek tıkla canlıya alma şablonu
└── Banking.sln                # Ana çözüm dosyası
```

---

## 📡 API Uç Noktaları

| Metot | Endpoint | Açıklama |
|---|---|---|
| `POST` | `/api/auth/login` | Müşteri girişi ve JWT token üretimi |
| `POST` | `/api/auth/register` | Yeni müşteri kaydı |
| `GET` | `/api/customers` | Tüm müşterileri listeleme |
| `GET` | `/api/customers/{id}` | Müşteri detayını getirme |
| `POST` | `/api/accounts` | Yeni hesap açma (otomatik TR IBAN) |
| `GET` | `/api/accounts/{id}` | Hesap detaylarını getirme |
| `GET` | `/api/accounts/{id}/balance` | Hesap bakiye sorgulama (CQRS Query) |
| `POST` | `/api/accounts/{id}/deposit` | Hesaba para yatırma (CQRS Command) |
| `GET` | `/api/accounts/{id}/transactions` | Hesap hareketlerini listeleme |
| `POST` | `/api/transactions/transfer` | İki hesap arası **atomik transfer** (CQRS Command & Unit of Work) |
