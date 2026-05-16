# Nimble Modulith

Навчальний проект — модульний моноліт на .NET 10 з Aspire, FastEndpoints, Mediator та Star Schema репортингом.

## Архітектура

Система побудована за патерном **Modulith** — всі модулі живуть в одному процесі, але комунікують виключно через публічні контракти (Mediator команди, запити, події), не звертаючись до внутрішніх деталей одне одного.

### Модулі

| Модуль | Опис | База даних |
|--------|------|------------|
| **Users** | Реєстрація, логін, ролі, скидання паролю | `usersdb` |
| **Products** | CRUD продуктів з цінами | `productsdb` |
| **Customers** | Клієнти, замовлення, підтвердження | `customersdb` |
| **Email** | Асинхронна відправка email через фонову чергу | — |
| **Reporting** | Star Schema аналітика підтверджених замовлень | `reportingdb` |

### Ключові патерни

- **Cross-module queries** — `GetProductDetailsQuery` дозволяє модулю Customers отримувати ціни з Products через Mediator, без прямого посилання на реалізацію
- **Domain Events** — `OrderCreatedEvent` публікується при підтвердженні замовлення; Reporting модуль підписується і ingests дані у Star Schema незалежно
- **Integration Handlers** — зовнішні контракти (`Users.Contracts`) відокремлені від внутрішньої реалізації через окремий Integration handler
- **Star Schema** — `FactOrders` + `DimDate` + `DimCustomer` + `DimProduct` для OLAP запитів через Dapper
- **Idempotency** — унікальний індекс `(OrderId, OrderItemId)` у `FactOrders` захищає від дублювання при повторній обробці події
- **EF Core + Dapper** — EF Core для запису та управління схемою, Dapper для швидких аналітичних читань

## Технологічний стек

- **.NET 10** + **ASP.NET Core**
- **.NET Aspire 13** — оркестрація, service discovery, observability
- **FastEndpoints 8** — мінімальні API ендпоінти
- **Mediator** (source-generated) — CQRS всередині процесу
- **Entity Framework Core 10** — ORM для запису
- **Dapper** — мікро-ORM для репортингу
- **SQL Server** — окрема БД на кожен модуль
- **MailKit** + **Papercut** — SMTP email та локальний перехоплювач
- **Serilog** — структуроване логування
- **Ardalis.Result** + **Ardalis.Specification** — результати операцій та специфікації

## Вимоги

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (для SQL Server та Papercut через Aspire)
- Visual Studio 2022 / Rider / VS Code

## Запуск

```bash
# Клонувати репозиторій
git clone <repo-url>
cd nimble-modulith

# Запустити через Aspire AppHost (рекомендовано)
cd Nimble.Modulith.AppHost
dotnet run
```

Aspire автоматично:
- Піднімає контейнер SQL Server
- Створює бази даних `usersdb`, `productsdb`, `customersdb`, `reportingdb`
- Піднімає Papercut (UI: http://localhost:37408, SMTP: localhost:25)
- Застосовує міграції та seed data
- Запускає Web API

Aspire Dashboard доступний за посиланням у консолі (зазвичай https://localhost:17018).

### Запуск без Aspire

```bash
# Потрібен локальний SQL Server і змінні середовища
cd Nimble.Modulith.Web
dotnet run
```

Swagger UI: http://localhost:5035/swagger

## Тестування API

### Базовий сценарій (через Swagger або .http файл)

```http
### 1. Створити продукт (потребує роль Admin)
POST http://localhost:5035/products
Content-Type: application/json

{ "name": "Ноутбук", "description": "Потужний", "price": 999.99 }

### 2. Створити клієнта (автоматично створює user + надсилає email)
POST http://localhost:5035/customers
Content-Type: application/json

{
  "firstName": "Іван", "lastName": "Петренко",
  "email": "ivan@test.com", "phoneNumber": "+380501234567",
  "address": { "street": "вул. Шевченка 1", "city": "Київ",
    "state": "Київська", "postalCode": "01001", "country": "Україна" }
}

### 3. Створити замовлення (ціна береться з Products автоматично)
POST http://localhost:5035/orders
Content-Type: application/json

{ "customerId": 1, "orderDate": "2026-05-16",
  "items": [{ "productId": 1, "quantity": 2 }] }

### 4. Підтвердити замовлення (публікує OrderCreatedEvent → Reporting ingestion)
POST http://localhost:5035/orders/1/confirm

### 5. Переглянути звіт
GET http://localhost:5035/reports/orders?startDate=2026-01-01&endDate=2026-12-31

### 6. Звіт у CSV
GET http://localhost:5035/reports/orders?startDate=2026-01-01&endDate=2026-12-31&format=csv

### 7. Скинути пароль
POST http://localhost:5035/users/reset-password
Content-Type: application/json

{ "email": "ivan@test.com" }
```

## API Ендпоінти

### Users
| Метод | Шлях | Опис |
|-------|------|------|
| POST | `/register` | Реєстрація |
| POST | `/login` | Логін |
| POST | `/logout` | Вихід |
| POST | `/users/{id}/roles` | Додати роль |
| POST | `/users/reset-password` | Скинути пароль |

### Products
| Метод | Шлях | Опис |
|-------|------|------|
| GET | `/products` | Список продуктів |
| GET | `/products/{id}` | Деталі продукту |
| POST | `/products` | Створити (Admin) |
| PUT | `/products/{id}` | Оновити (Admin) |
| DELETE | `/products/{id}` | Видалити (Admin) |

### Customers & Orders
| Метод | Шлях | Опис |
|-------|------|------|
| POST | `/customers` | Створити клієнта |
| GET | `/customers/{id}` | Деталі клієнта |
| POST | `/orders` | Створити замовлення |
| GET | `/orders/{id}` | Деталі замовлення |
| POST | `/orders/{id}/items` | Додати позицію |
| POST | `/orders/{id}/confirm` | Підтвердити замовлення |

### Reports
| Метод | Шлях | Опис |
|-------|------|------|
| GET | `/reports/orders` | Звіт по замовленнях (JSON/CSV) |
| GET | `/reports/product-sales` | Продажі по продуктах (JSON/CSV) |
| GET | `/reports/customers/{id}/orders` | Замовлення клієнта (JSON/CSV) |

## Структура проекту

```
├── Nimble.Modulith.AppHost/          # Aspire оркестрація
├── Nimble.Modulith.ServiceDefaults/  # Спільні Aspire сервіси
├── Nimble.Modulith.Web/              # ASP.NET Core хост + Program.cs
│
├── Nimble.Modulith.Users.Contracts/  # Публічний контракт Users модуля
├── Nimble.Modulith.Users/            # Реалізація Users модуля
│
├── Nimble.Modulith.Products.Contracts/
├── Nimble.Modulith.Products/
│
├── Nimble.Modulith.Customers.Contracts/  # OrderCreatedEvent тут
├── Nimble.Modulith.Customers/
│
├── Nimble.Modulith.Email.Contracts/  # SendEmailCommand
├── Nimble.Modulith.Email/            # SMTP + Channel queue
│
├── Nimble.Modulith.Reporting/        # Star Schema + Dapper
│
└── docs/                             # C4 діаграми
```

## Важливі архітектурні рішення

**Чому один процес?** Modulith дозволяє зберегти чіткі межі модулів (як у мікросервісах) без overhead мережевих викликів, серіалізації та розподіленої транзакційності. Легко розбити на сервіси пізніше.

**Чому Mediator замість прямих викликів?** Модулі не знають про реалізацію одне одного — тільки про контракти. `Customers` модуль може отримати ціну продукту не імпортуючи `Products` проект.

**Чому окрема БД для Reporting?** Star Schema оптимізована для читання (OLAP), а транзакційні БД — для запису (OLTP). Розділення дозволяє додавати індекси, матеріалізовані вью та аналітичні запити не впливаючи на основну систему.

**Чому EnsureDeletedAsync для Reporting у dev?** Seed data (365 дат) додається через `HasData()` і застосовується тільки при створенні бази. Drop+Create гарантує чисті дані при розробці.
