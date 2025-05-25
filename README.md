# ProjectOne - Sosyal Medya API

## Kurulum

### 1. Proje Klonlama

git clone https://github.com/yourusername/projectone.git
cd projectone

### 2. Yapılandırma
# appsettings dosyasını kopyala
cp appsettings.Example.json appsettings.json

# Kendi değerlerinizi girin
nano appsettings.json

### 3. Gereksinimler

.NET 8.0+
MongoDB
Visual Studio veya VS Code

### 4. Çalıştırma

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