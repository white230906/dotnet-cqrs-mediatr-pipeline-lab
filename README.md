# .NET CQRS MediatR Pipeline Lab

Mini-project thực hành CQRS, MediatR và Pipeline Behaviors trong ASP.NET Core 8 với PostgreSQL.

Mục tiêu của project là chuyển từ Application Service kiểu 3-tier sang cách tổ chức từng use case bằng Command, Query và Handler; đồng thời đưa validation, logging, performance measurement và transaction ra khỏi business handler.

## Technical stack

- .NET 8
- ASP.NET Core Minimal API
- Carter
- MediatR
- FluentValidation
- Entity Framework Core
- PostgreSQL
- Serilog
- Swagger/OpenAPI
- xUnit và Testcontainers

## Request flow

```text
HTTP Request
→ Carter Endpoint
→ ISender.Send
→ Validation Behavior
→ Request Logging Behavior
→ Performance Behavior
→ Transaction Behavior
→ Handler
→ Repository
→ PostgreSQL
```

Pipeline behaviors được đăng ký theo thứ tự có chủ đích:

1. Validation kiểm tra input và dừng sớm nếu không hợp lệ.
2. Logging ghi nhận request và kết quả xử lý.
3. Performance đo thời gian thực thi của use case.
4. Transaction quản lý save, commit và rollback cho Command.

## CQRS structure

- **Command:** yêu cầu thay đổi trạng thái hệ thống.
- **Query:** yêu cầu đọc dữ liệu, không thay đổi trạng thái hệ thống.
- **Handler:** xử lý một use case cụ thể.
- **ISender:** gửi Command/Query đến đúng Handler thông qua MediatR.
- **Result:** biểu diễn kết quả thành công hoặc lỗi nghiệp vụ có chủ đích.

Ví dụ:

```text
Command.CreateWorkItem
→ CreateWorkItem Handler

Command.ChangeWorkItemStatus
→ ChangeWorkItemStatus Handler

Query.GetWorkItemById
→ GetWorkItemById Handler
```

## CQRS giải quyết gì trong lab?

- Tách use case đọc và ghi rõ ràng.
- Mỗi use case có Handler và dependency riêng.
- Tránh Application Service phình to khi nghiệp vụ tăng.
- Đưa validation, logging và transaction ra khỏi Handler.
- Cho phép áp dụng chính sách riêng: Command dùng transaction, Query bỏ qua transaction ghi.

CQRS trong project này không tự động giải quyết database bottleneck, không thay thế query optimization và không bắt buộc phải dùng cho ứng dụng CRUD nhỏ.

## Project structure

```text
Api
├── Composition root
├── Middleware
└── Dependency injection

Presentation
├── Carter endpoints
└── HTTP Result mapping

Application
├── Commands / Queries / Handlers
├── Validators
├── Pipeline Behaviors
└── Use-case mapping

Contract
├── Command / Query messages
├── Request / Response contracts
└── Result / Error abstractions

Domain
├── WorkItem entities
├── Business rules
├── Repository abstractions
└── Unit of Work abstraction

Persistence
├── EF Core DbContext
├── PostgreSQL repositories
├── Entity configurations
└── Migrations
```

Dependency direction chính:

```text
Presentation → Contract
Application  → Contract + Domain
Persistence  → Domain
Api          → Application + Presentation + Persistence
```

## WorkItem API

| Method | Endpoint | Mục đích |
|---|---|---|
| `GET` | `/api/v1/work-items` | Lấy danh sách WorkItem |
| `GET` | `/api/v1/work-items/{id}` | Lấy WorkItem theo ID |
| `POST` | `/api/v1/work-items` | Tạo WorkItem |
| `PUT` | `/api/v1/work-items/{id}/status` | Chuyển trạng thái WorkItem |
| `GET` | `/api/v1/work-items/{id}/history` | Lấy lịch sử trạng thái |

Luồng trạng thái hợp lệ:

```text
Pending → InProgress → Completed
```

Project sử dụng PostgreSQL `xmin` làm concurrency token để phát hiện stale update.

## Run locally

Yêu cầu:

- .NET SDK 8
- PostgreSQL

Cấu hình connection string `ConnectionStrings:PostgresSql` bằng local configuration hoặc user secrets. Không commit credential thật lên Git.

Restore và build:

```powershell
dotnet restore
dotnet build .\dotnet-cqrs-mediatr-pipeline-lab.sln
```

Apply migration:

```powershell
dotnet ef database update `
    --project .\dotnet-cqrs-mediatr-pipeline-lab.Persistence `
    --startup-project .\dotnet-cqrs-mediatr-pipeline-lab.Api
```

Chạy API:

```powershell
dotnet run `
    --project .\dotnet-cqrs-mediatr-pipeline-lab.Api
```

Mở Swagger theo URL được hiển thị trong terminal khi ứng dụng chạy ở Development environment.

## Integration tests

Integration tests sử dụng PostgreSQL thật thông qua Testcontainers. Docker phải đang chạy trước khi test:

```powershell
dotnet test `
    .\dotnet-cqrs-mediatr-pipeline-lab.Api.IntegrationTests\dotnet-cqrs-mediatr-pipeline-lab.Api.IntegrationTests.csproj
```

## Technical takeaways

- MediatR dispatch request dựa trên request type và handler registration.
- Assembly scanning tự tìm Handler và Validator trong Application assembly.
- Pipeline Behavior tương tự middleware nhưng bao quanh Command/Query thay vì toàn bộ HTTP pipeline.
- Validation có thể short-circuit và không gọi Handler.
- `ICommandBase` đánh dấu request cần transaction.
- Query không mở transaction ghi.
- Command thành công thì save và commit; lỗi hoặc exception thì rollback.
- Correlation ID middleware và MediatR logging behavior phục vụ hai phạm vi quan sát khác nhau.

## Scope

Đây là project học tập technical-first. Project tập trung vào CQRS, MediatR, Pipeline Behaviors, EF Core transaction và optimistic concurrency; không nhằm mô phỏng toàn bộ nghiệp vụ của một hệ thống production.
