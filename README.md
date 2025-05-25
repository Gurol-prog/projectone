# ProjectOne - Yorum Sistemi API

## Kurulum


# appsettings dosyasını Kendi değerlerinizi girin


### 1. Gereksinimler

.NET 7.0.5+
MongoDB
Visual Studio veya VS Code

### 2. Çalıştırma

dotnet restore
dotnet run

# API Endpoints
Authentication

POST /api/Auth/login - Giriş
POST /api/Auth/refresh - Token yenileme

## Content

GET /api/Content - İçerikleri listele
POST /api/Content - İçerik oluştur

## Comments

GET /api/Comments/content/{id} - Yorumları getir
POST /api/Comments/content/{id} - Ana yorum ekle
POST /api/Comments/{commentId}/reply - Yanıt ekle

## Güvenlik

JWT Authentication
MongoDB ile NoSQL
Kullanıcı yetkilendirme sistemi

## 4. Klasör Yapısını Düzenleyin

ProjectOne/

├── Controllers/

├── Services/

├── Models/

├── Dtos/

├── Config/

├── appsettings.Example.json

├── .gitignore

├── README.md

└── Program.cs
