# Task & Project Management API (NET 9)

Bu proje, kullanıcıların projeler oluşturabildiği, projeler altında görevler (task) tanımlayabildiği ve görevler üzerine yorum yapabildiği bir **RESTful API** uygulamasıdır.

Proje, **.NET 9**, **Entity Framework Core**, **SQLite** ve **JWT (JSON Web Token)** tabanlı bir mimari kullanmaktadır.

---

## Proje Özellikleri

- Kullanıcı yönetimi (Admin / User)
- Proje oluşturma ve yönetme
- Proje bazlı görev (Task) takibi
- Görevler için yorum sistemi
- JWT tabanlı Authentication & Authorization
- Role-based access control (RBAC)
- Katmanlı mimari (Controller / Service / Data)
- Seed data ile initial veriler
- Soft delete desteği
- Audit trail (CreatedAt, UpdatedAt, DeletedAt)

---

### Kimlik Doğrulama (JWT)

Sistemde JWT tabanlı authentication kullanılmaktadır.

#### Roller

- Admin
- User

#### Yetkilendirme Kuralları

- Tüm endpoint’ler JWT token gerektirir (login hariç)
- Admin kullanıcılar tüm CRUD işlemlerini yapabilir
- Normal kullanıcılar yalnızca okuma işlemleri yapabilir

## Mimari Yapı

Proje **Controller – Service – Data** katmanlı mimari kullanılarak geliştirilmiştir.

```text
Client (Swagger / Frontend)
        |
        v
Controllers (API Endpoints)
        |
        v
Services (Business Logic)
        |
        v
Entity Framework Core
        |
        v
SQLite Database
```

Controllers

- HTTP isteklerini karşılar, yetkilendirme kontrollerini yapar ve servisleri çağırır.

Services

- İş mantığını içerir. Veritabanı işlemleri burada yapılır.

Entities

- Veritabanı tablolarını temsil eder.

DTOs

- API istek ve cevapları için kullanılan veri transfer nesneleridir.

## Entity İlişkileri

```bash

Users
    ↳ Projects (1-N)
        ↳ TasksItems (1-N)
            ↳ Comments (1-N)
```

Bir kullanıcı birden fazla proje oluşturabilir

Bir proje birden fazla görev içerir

Bir görev birden fazla yorum alabilir

Bir yorum bir kullanıcıya aittir

## Endpoint Listesi

### AUTH

| Method | Endpoint    | Açıklama                      |
| ------ | ----------- | ----------------------------- |
| POST   | /auth/login | Kullanıcı girişi (JWT üretir) |

### Users

| Method | Endpoint         | Yetki        |
| ------ | ---------------- | ------------ |
| GET    | /users           | Admin / User |
| GET    | /users/{id}      | Admin / User |
| POST   | /users           | Admin        |
| PUT    | /users/{id}/role | Admin        |
| DELETE | /users/{id}      | Admin        |

### Projects

| Method | Endpoint                | Yetki        |
| ------ | ----------------------- | ------------ |
| GET    | /projects               | Admin / User |
| GET    | /projects/{id}          | Admin / User |
| GET    | /projects/user/{userId} | Admin / User |
| POST   | /projects               | Admin        |
| PUT    | /projects/{id}          | Admin        |
| DELETE | /projects/{id}          | Admin        |

### Tasks

| Method | Endpoint                    | Yetki        |
| ------ | --------------------------- | ------------ |
| GET    | /projects/{projectId}/tasks | Admin / User |
| POST   | /projects/{projectId}/tasks | Admin        |
| PUT    | /tasks/{id}                 | Admin        |
| DELETE | /tasks/{id}                 | Admin        |

### Comments

| Method | Endpoint                 | Yetki        |
| ------ | ------------------------ | ------------ |
| GET    | /tasks/{taskId}/comments | Admin / User |
| POST   | /tasks/{taskId}/comments | Admin        |
| PUT    | /comments/{id}           | Admin        |
| DELETE | /comments/{id}           | Admin        |

## API Response Örnekleri

### Başarılı Response

```json
{
  "success": true,
  "message": "Project created",
  "data": {
    "id": 4,
    "name": "CRM (RBAC)",
    "description": "Do the RBAC part in the CRM project.",
    "userId": 5
  }
}
```

### Hatalı Response

```json
{
  "success": false,
  "message": "Project not found",
  "data": null
}
```

## Kurulum Talimatları

```bash
git clone https://github.com/sherif-major/DotNet-REST-TaskManager
cd DotNet-REST-TaskManager
dotnet restore
dotnet run --project Api
```

### Varsayılan Admin Kullanıcı (Seed data'dan gelen):

- Username: admin
- Password: admin123

### Swagger arayüzüne erişmek için:

- <http://localhost:{PORT}/swagger>

### Uygulamayı Çalıştırmak İçin:

```bash
dotnet build
dotnet run --project Api
```
