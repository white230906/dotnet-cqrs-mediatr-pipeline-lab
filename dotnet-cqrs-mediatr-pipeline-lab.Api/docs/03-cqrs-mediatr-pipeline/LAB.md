# Mini-project 03: CQRS, MediatR & Pipeline Behaviors

## 1. Mục tiêu

Mở rộng source đã hoàn thành ở Mini-project 02 để thay cách điều phối use case qua `IService/Service` bằng CQRS và MediatR.

Technical core của lab:

- Phân biệt rõ `Command` và `Query`.
- Mỗi use case có một request và một handler riêng.
- Carter endpoint chỉ chuyển HTTP input thành command/query rồi gửi qua `ISender`.
- Tự động tìm handler bằng assembly scanning.
- Đưa validation ra khỏi handler bằng FluentValidation và `IPipelineBehavior`.
- Đưa transaction ra khỏi từng command handler bằng transaction behavior.
- Đo thời gian và ghi log request bằng pipeline behavior.
- Hiểu thứ tự behavior và cơ chế gọi `next()`.
- Kiểm chứng validation short-circuit, transaction rollback và cancellation bằng test.

Sau lab này, bạn phải giải thích được đường đi:

```text
HTTP request
→ Carter endpoint
→ ISender.Send(command/query)
→ Validation behavior
→ Performance/logging behavior
→ Transaction behavior (chỉ command)
→ Request handler
→ Repository + Unit of Work
→ PostgreSQL
→ Result
→ HTTP response
```

## 2. Production motivation và bằng chứng

Các cơ chế trong lab được chọn từ những phần đã xác minh trong `buildx-be`:

- Đăng ký MediatR, validators và pipeline behaviors:
  `src/bmt-be.application/dependencyInjection/extensions/ServiceCollectionExtensions.cs`, method `AddMediatRApplication`.
- Abstraction cho command/query:
  `src/bmt-be.contract/abstractions/messages/ICommand.cs`,
  `ICommandHandler.cs`, `IQuery.cs`, `IQueryHandler.cs`.
- Validation chạy trước handler:
  `src/bmt-be.application/behaviors/ValidationPipelineBehavior.cs`.
- Transaction bao quanh command:
  `src/bmt-be.application/behaviors/TransactionPipelineBehavior.cs`.
- Đo request chậm:
  `src/bmt-be.application/behaviors/PerformancePipelineBehavior.cs`.
- Ghi log thời gian thực thi:
  `src/bmt-be.application/behaviors/TracingPipelineBehavior.cs`.
- Presentation gửi request bằng `ISender`:
  ví dụ `src/bmt-be.presentation/apis/architect/ArchitectApi.cs`.
- API composition root gọi `AddMediatRApplication()`:
  `src/bmt-be.api/Program.cs`.

Đây là bằng chứng về convention của dự án, không có nghĩa mọi lựa chọn đều là quy tắc bắt buộc cho tất cả hệ thống.

Lab tái tạo cơ chế kỹ thuật, không sao chép nghiệp vụ, payload hoặc implementation riêng của production.

## 3. Source dùng làm điểm xuất phát

Sao chép source đã hoàn thành của Mini-project 02 sang repository mới.

Tên repo đề xuất:

```text
dotnet-cqrs-mediatr-pipeline-lab
```

Giữ lại toàn bộ phần đã hoàn thành:

- API host, Carter, Swagger và API versioning.
- Domain `WorkItem` và quy tắc chuyển trạng thái.
- Contract request/response hiện tại.
- PostgreSQL, EF Core, migration và seed data.
- Repository abstraction và implementation.
- Unit of Work và explicit transaction.
- Optimistic concurrency bằng PostgreSQL `xmin`.
- Correlation ID, Problem Details và Serilog.
- Integration test bằng `WebApplicationFactory` + Testcontainers.

Phần phải thay đổi:

- Endpoint không còn inject `WorkItems.IService`.
- Tách từng method của service thành command/query handler.
- Validation chuyển sang FluentValidation pipeline.
- Transaction của use case chuyển trạng thái chuyển sang pipeline behavior.
- Đăng ký MediatR, validators và behaviors tại Application DI extension.
- Bổ sung test chứng minh pipeline thực sự chạy.

Không xóa code service cũ ngay. Chỉ xóa sau khi endpoint, handler và test mới đều hoạt động để dễ đối chiếu trước/sau.

## 4. Bài toán tối thiểu

Tiếp tục dùng domain trung lập của Lab 2:

```text
WorkItem
- Id          Guid
- Title       string
- Status      Pending | InProgress | Completed
- CreatedAt   DateTimeOffset
- Version     uint

WorkItemStatusHistory
- Id
- WorkItemId
- PreviousStatus
- NewStatus
- ChangedAt
```

Giữ nguyên quy tắc:

- WorkItem mới luôn có trạng thái `Pending`.
- Title bắt buộc, sau khi trim phải nằm trong giới hạn đã chọn ở Lab 1.
- Chỉ cho phép `Pending → InProgress → Completed`.
- Mỗi lần đổi trạng thái hợp lệ phải tạo history.
- WorkItem và history phải cùng commit hoặc cùng rollback.
- Update phải kiểm tra concurrency version.

Lab 3 không bổ sung entity hoặc business rule mới. Mục tiêu là thay đổi cách tổ chức và thực thi use case.

## 5. API contract

Giữ nguyên API từ Lab 2:

| Method | Route | MediatR request |
|---|---|---|
| `GET` | `/api/v1/work-items` | `GetWorkItemsQuery` |
| `GET` | `/api/v1/work-items/{id}` | `GetWorkItemByIdQuery` |
| `POST` | `/api/v1/work-items` | `CreateWorkItemCommand` |
| `PUT` | `/api/v1/work-items/{id}/status` | `ChangeWorkItemStatusCommand` |
| `GET` | `/api/v1/work-items/{id}/history` | `GetWorkItemHistoryQuery` |

HTTP body DTO và MediatR request không bắt buộc phải là cùng một type.

Ví dụ luồng update:

```text
UpdateWorkItemStatusRequest (HTTP contract)
→ endpoint lấy thêm route id
→ ChangeWorkItemStatusCommand(id, status, version)
→ sender.Send(command, cancellationToken)
```

Route parameter thuộc HTTP boundary, vì vậy endpoint chịu trách nhiệm ghép nó với body thành command hoàn chỉnh.

## 6. Command và Query trong lab

### Query

Query chỉ đọc dữ liệu:

- `GetWorkItemsQuery`
- `GetWorkItemByIdQuery`
- `GetWorkItemHistoryQuery`

Quy tắc:

- Không thay đổi entity.
- Không gọi `SaveChangesAsync`.
- Không mở explicit transaction trong lab này.
- Repository dùng `AsNoTracking()` cho luồng chỉ đọc.
- Trả response model, không trả EF entity ra Presentation.

### Command

Command biểu diễn ý định thay đổi hệ thống:

- `CreateWorkItemCommand`
- `ChangeWorkItemStatusCommand`

Quy tắc:

- Tên command mô tả hành động, không đặt theo HTTP verb như `PostWorkItemCommand`.
- Handler điều phối domain và repository.
- Handler không biết HTTP status code.
- Handler không tự tạo transaction khi transaction behavior đã chịu trách nhiệm đó.
- Command nhận `CancellationToken` thông qua handler.

CQRS trong lab là tách model thực thi đọc và ghi. Không có yêu cầu tách database đọc/ghi hoặc dùng event sourcing.

## 7. Technical constraints

- .NET 8.
- MediatR `12.4.0` cho các project cần dùng MediatR types.
- FluentValidation `12.1.1`.
- FluentValidation.DependencyInjectionExtensions `12.1.1`.
- Dùng cùng một major/minor MediatR giữa các project; không cố tình sao chép version lệch nhau từ production.
- Application đăng ký handler bằng assembly scanning.
- Presentation chỉ inject `ISender`, không inject handler cụ thể.
- Mỗi request chỉ có một handler.
- Mọi async handler nhận và truyền tiếp `CancellationToken`.
- Validation không được viết lặp lại trong Carter endpoint và handler.
- Invalid request không được chạy handler và không được mở transaction.
- Query không được mở transaction behavior.
- Command ghi dữ liệu phải nằm trong transaction boundary.
- Không inject `AppDbContext` vào handler hoặc Presentation.
- Không dùng MediatR notification, domain event, outbox hoặc message broker trong lab này.
- Không thay repository bằng generic repository.
- Giữ toàn bộ integration test quan trọng của Lab 2.

## 8. Cấu trúc đích

Điều chỉnh tên root namespace theo tên solution thực tế của bạn.

```text
dotnet-cqrs-mediatr-pipeline-lab/
├── *.Api/
│   ├── DependencyInjection/
│   └── Program.cs
├── *.Presentation/
│   ├── Abstractions/
│   └── Apis/
│       └── WorkItem.cs
├── *.Contract/
│   ├── Abstractions/
│   │   ├── Messages/
│   │   │   ├── ICommand.cs
│   │   │   ├── ICommandHandler.cs
│   │   │   ├── IQuery.cs
│   │   │   └── IQueryHandler.cs
│   │   └── Results/
│   │       ├── Error.cs
│   │       ├── Result.cs
│   │       └── ResultOfT.cs
│   └── WorkItems/
│       ├── Requests/
│       ├── Responses/
│       ├── Commands/
│       │   ├── CreateWorkItemCommand.cs
│       │   └── ChangeWorkItemStatusCommand.cs
│       ├── Queries/
│       │   ├── GetWorkItemsQuery.cs
│       │   ├── GetWorkItemByIdQuery.cs
│       │   └── GetWorkItemHistoryQuery.cs
│       └── Validators/
│           ├── CreateWorkItemCommandValidator.cs
│           └── ChangeWorkItemStatusCommandValidator.cs
├── *.Application/
│   ├── Behaviors/
│   │   ├── ValidationPipelineBehavior.cs
│   │   ├── RequestLoggingPipelineBehavior.cs
│   │   ├── PerformancePipelineBehavior.cs
│   │   └── TransactionPipelineBehavior.cs
│   ├── DependencyInjection/
│   │   └── ApplicationServiceCollectionExtensions.cs
│   └── WorkItems/
│       ├── Commands/
│       │   ├── CreateWorkItemCommandHandler.cs
│       │   └── ChangeWorkItemStatusCommandHandler.cs
│       └── Queries/
│           ├── GetWorkItemsQueryHandler.cs
│           ├── GetWorkItemByIdQueryHandler.cs
│           └── GetWorkItemHistoryQueryHandler.cs
├── *.Domain/
├── *.Persistence/
└── *.Api.IntegrationTests/
```

Không bắt buộc tên folder giống tuyệt đối. Điều bắt buộc là dependency direction và trách nhiệm của từng type phải rõ.

## 9. Project references và packages

Giữ dependency direction:

```text
API → Application + Persistence + Presentation
Presentation → Application + Contract
Application → Contract + Domain
Persistence → Domain
Contract → không phụ thuộc Application/Persistence/Presentation
Domain → không phụ thuộc layer ngoài
IntegrationTests → API
```

Package cần thêm:

### Contract

```xml
<PackageReference Include="MediatR" Version="12.4.0" />
<PackageReference Include="FluentValidation" Version="12.1.1" />
```

Contract cần MediatR vì command/query implement `IRequest<TResponse>`. Validator đặt trong Contract nên cần FluentValidation.

### Application

```xml
<PackageReference Include="MediatR" Version="12.4.0" />
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
```

Application chứa handlers, behaviors và assembly scanning.

### Presentation

```xml
<PackageReference Include="MediatR" Version="12.4.0" />
```

Presentation cần `ISender` để gửi command/query.

Sau khi thêm package, kiểm tra không có hai version MediatR khác nhau trong dependency graph.

## 10. Checkpoint 1 — Message abstractions

Tạo bốn abstraction trong Contract:

```text
ICommand
ICommand<TResponse>
ICommandHandler<TCommand>
ICommandHandler<TCommand, TResponse>
IQuery<TResponse>
IQueryHandler<TQuery, TResponse>
```

Quan hệ cần đạt:

```text
ICommand<TResponse>
→ IRequest<Result<TResponse>>

IQuery<TResponse>
→ IRequest<Result<TResponse>>

ICommandHandler<TCommand, TResponse>
→ IRequestHandler<TCommand, Result<TResponse>>

IQueryHandler<TQuery, TResponse>
→ IRequestHandler<TQuery, Result<TResponse>>
```

Nếu chưa muốn dùng generic abstraction ngay, có thể bắt đầu trực tiếp với `IRequest<TResponse>` và `IRequestHandler<TRequest,TResponse>`, sau đó refactor. Tuy nhiên checkpoint chỉ hoàn thành khi đã hiểu giá trị của abstraction riêng cho command/query.

Điều kiện qua checkpoint:

- Command và query có thể được compiler phân biệt bằng interface.
- Handler interface buộc đúng response type.
- Contract không tham chiếu Application.
- Giải thích được tại sao `ISender` không cần biết concrete handler.

## 11. Checkpoint 2 — Result và Error contract tối thiểu

Tạo Result model đủ dùng cho lab:

```text
Error
- Code
- Message

Result
- IsSuccess
- IsFailure
- Errors

Result<T>
- Value
- IsSuccess
- IsFailure
- Errors
```

Factory behavior tối thiểu:

- Success không chứa error.
- Failure chứa ít nhất một error.
- Không đọc `Value` khi result thất bại.
- Validation có thể trả nhiều error.

Mục đích:

- Handler mô tả kết quả use case mà không phụ thuộc HTTP.
- Validation behavior có thể short-circuit bằng failure result.
- Presentation quyết định mapping Result sang `200/201/404/409/422`.

Không biến Result thành framework lớn. Chỉ hỗ trợ behavior thực sự cần trong lab.

Failure experiment:

- Cố tạo một `Result<T>` vừa success vừa có error.
- Nếu API thiết kế cho phép trạng thái vô lý này, sửa constructor/factory để invariant được bảo vệ.

## 12. Checkpoint 3 — Khai báo command, query và validator

Tạo năm MediatR request trong Contract.

Nội dung tối thiểu:

```text
CreateWorkItemCommand
- Title

ChangeWorkItemStatusCommand
- Id
- NewStatus
- ExpectedVersion

GetWorkItemsQuery
- không có input

GetWorkItemByIdQuery
- Id

GetWorkItemHistoryQuery
- WorkItemId
```

Tạo validator cho hai command:

- Create: title không rỗng sau trim và không vượt giới hạn.
- Change status: ID khác `Guid.Empty`, expected version hợp lệ và enum hợp lệ.

Validator chỉ kiểm tra shape/input rule có thể xác định mà không cần database.

Không đưa các rule sau vào FluentValidation:

- WorkItem có tồn tại hay không.
- Transition hiện tại có hợp lệ hay không.
- Expected version có khớp database hay không.

Các rule này cần state hiện tại nên thuộc handler/domain/persistence flow.

Điều kiện qua checkpoint:

- Command/query là immutable record hoặc type tương đương.
- Validator không gọi repository.
- Không còn validation title lặp lại trong endpoint.

## 13. Checkpoint 4 — Query handlers

Implement lần lượt:

1. `GetWorkItemsQueryHandler`.
2. `GetWorkItemByIdQueryHandler`.
3. `GetWorkItemHistoryQueryHandler`.

Mỗi handler chỉ làm các bước cần thiết:

```text
Nhận query
→ gọi repository read method
→ map entity/read model sang response
→ trả Result<T>
```

Quy tắc:

- Truyền `CancellationToken` xuống repository.
- Không gọi Unit of Work.
- Không mở transaction.
- Không return `IQueryable` hoặc entity ra ngoài Application.
- Query by ID không tồn tại trả failure với error code ổn định, ví dụ `WorkItem.NotFound`.

Test checkpoint:

- Query list trả dữ liệu đúng thứ tự.
- Query by ID tồn tại trả success.
- Query by ID không tồn tại trả failure `WorkItem.NotFound`.
- Verify Unit of Work không được gọi bởi query.

## 14. Checkpoint 5 — Command handlers

Implement:

### CreateWorkItemCommandHandler

Trách nhiệm:

```text
Nhận command
→ gọi domain factory để tạo WorkItem
→ repository.AddAsync
→ map response
```

### ChangeWorkItemStatusCommandHandler

Trách nhiệm:

```text
Nhận command
→ load tracked WorkItem
→ trả NotFound nếu không có
→ kiểm tra expected version
→ gọi domain ChangeStatus
→ thêm WorkItemStatusHistory
→ map response
```

Chưa mở transaction trong handler. Transaction behavior sẽ bao quanh toàn bộ handler ở checkpoint sau.

Không chuyển business rule từ Domain ngược lên handler. Handler chỉ điều phối use case và chuyển domain failure thành Result phù hợp.

Điều kiện qua checkpoint:

- Handler không biết Carter, HTTP hoặc status code.
- Handler không inject `AppDbContext`.
- Handler không gọi handler khác qua `ISender`.
- Mỗi handler có một trách nhiệm use case rõ ràng.

## 15. Checkpoint 6 — Đăng ký MediatR bằng assembly scanning

Tạo Application DI extension, ví dụ:

```text
AddApplication()
```

Extension chịu trách nhiệm:

- Đăng ký MediatR.
- Scan assembly chứa handlers.
- Scan assembly chứa validators.
- Đăng ký pipeline behaviors theo thứ tự chủ đích.

Dùng một marker type như `AssemblyReference` để lấy assembly ổn định, không dùng một handler ngẫu nhiên làm marker.

Composition root chỉ gọi extension:

```text
builder.Services.AddApplication();
```

Failure experiment:

1. Tạm scan nhầm assembly.
2. Gửi một command.
3. Quan sát lỗi không tìm thấy handler.
4. Khôi phục assembly đúng và giải thích MediatR tìm handler như thế nào.

Điều kiện qua checkpoint:

- Không đăng ký thủ công từng handler.
- `ISender.Send` resolve được đủ năm request.
- DI extension nằm trong Application, API chỉ gọi extension.

## 16. Checkpoint 7 — Validation pipeline behavior

Implement open generic behavior:

```text
ValidationPipelineBehavior<TRequest, TResponse>
```

Cơ chế:

```text
Resolve tất cả IValidator<TRequest>
→ nếu không có validator: gọi next()
→ chạy validators
→ gom validation errors
→ nếu có lỗi: trả validation failure, không gọi next()
→ nếu hợp lệ: gọi next()
```

Yêu cầu:

- Behavior không biết cụ thể `CreateWorkItemCommand`.
- Một request có thể có nhiều validator.
- Error code và property name phải ổn định để test.
- Validation dùng async API nếu validator có async rule.
- Không log toàn bộ request nếu sau này request có dữ liệu nhạy cảm.

Thứ tự đăng ký:

```text
Validation
→ Logging/Performance
→ Transaction
→ Handler
```

Validation đặt ngoài transaction để invalid input không chiếm connection và không mở transaction vô ích.

Test quan trọng:

- Gửi title rỗng.
- Nhận validation failure/HTTP `422`.
- Handler không chạy.
- Repository không được gọi.
- Transaction không được bắt đầu.

## 17. Checkpoint 8 — Logging và performance behaviors

Tạo hai behavior nhỏ hoặc một behavior kết hợp có chủ đích.

### RequestLoggingPipelineBehavior

Ghi tối thiểu:

- Request type.
- Bắt đầu/kết thúc.
- Success/failure nếu Result hỗ trợ.
- Không ghi payload đầy đủ mặc định.

### PerformancePipelineBehavior

Cơ chế:

```text
Lấy timestamp bắt đầu
→ await next()
→ tính elapsed time trong finally hoặc sau next tùy mục tiêu
→ log Warning nếu vượt threshold
```

Ưu tiên inject `TimeProvider` để test deterministic. Threshold có thể dùng Options hoặc constant nhỏ trong lab.

Failure experiment:

- Tạo một test handler có delay vượt threshold.
- Xác nhận Warning được ghi một lần.
- Cho handler ném exception và kiểm tra behavior có ghi thời gian theo thiết kế của bạn hay không.

Cần giải thích rõ:

- Vì sao behavior không dùng `new Stopwatch()` rải rác trong từng handler.
- Vì sao không log trực tiếp password/token/request nhạy cảm.
- Khác nhau giữa correlation ID của HTTP middleware và request type/timing của MediatR behavior.

## 18. Checkpoint 9 — Transaction pipeline behavior

Chuyển transaction boundary từ command service/handler sang behavior.

Thêm marker `ICommand` hoặc `ITransactionalRequest` để behavior nhận biết request ghi dữ liệu. Không chỉ dựa vào suffix tên class nếu có thể tránh.

Cơ chế yêu cầu:

```text
Nếu request là query
→ gọi next(), không mở transaction

Nếu request là command
→ BeginTransactionAsync
→ gọi next()
→ nếu Result failure: rollback
→ nếu success: SaveChanges/Complete + commit
→ nếu exception/cancellation: rollback rồi throw lại
→ luôn dispose transaction resource
```

Phải đối chiếu API thật của `IUnitOfWork` ở Lab 2. Nếu Unit of Work tách `SaveChangesAsync` và `CommitAsync`, gọi đúng thứ tự. Nếu dùng `CompleteAsync`, ghi rõ method đó đang làm cả việc gì.

Quy tắc:

- Query không mở transaction.
- Validation failure không đi tới transaction behavior.
- Behavior không bắt exception rồi biến thành success.
- Rollback không được che mất exception gốc.
- Không giữ transaction trong lúc gọi external HTTP service.
- Không tự động retry command khi chưa chứng minh idempotency.

Failure experiment bắt buộc:

1. Command đổi WorkItem status.
2. Sau khi thay đổi WorkItem nhưng trước khi hoàn tất history, ném exception thử nghiệm.
3. Xác nhận transaction rollback.
4. Dùng DbContext mới kiểm tra status và history đều không bị ghi dở.

## 19. Checkpoint 10 — Refactor Carter endpoint sang ISender

Thay dependency của endpoint:

```text
Trước: WorkItems.IService
Sau:   MediatR.ISender
```

Mỗi endpoint làm tối đa bốn việc:

```text
Bind HTTP input
→ tạo command/query
→ sender.Send(request, cancellationToken)
→ map Result thành IResult
```

Mapping gợi ý:

| Result/Error | HTTP |
|---|---|
| Query/Create/Update success | `200` hoặc `201` |
| `WorkItem.NotFound` | `404` |
| `WorkItem.InvalidState` | `409` |
| `WorkItem.ConcurrencyConflict` | `409` |
| Validation errors | `422` |
| Unexpected exception | middleware xử lý thành `500` |

Không làm trong endpoint:

- Gọi repository.
- Gọi Unit of Work.
- Validate lại title.
- Bắt mọi exception bằng `try/catch` tổng quát.
- Resolve handler trực tiếp.

Giữ Swagger `.Produces<T>()` và `.ProducesProblem()` khớp với behavior thật.

## 20. Checkpoint 11 — Xóa service cũ có kiểm soát

Chỉ thực hiện khi tất cả endpoint đã chuyển sang `ISender` và test cũ vẫn chạy.

Checklist:

- Không còn endpoint inject `WorkItems.IService`.
- Không còn logic chỉ tồn tại trong service cũ mà chưa được chuyển sang handler/domain.
- Không còn DI registration cho `IService/Service` cũ.
- Không còn test chỉ gọi service cũ nhưng bỏ qua pipeline mới.
- Search toàn solution xác nhận service cũ không còn consumer.

Sau đó mới xóa:

```text
Application/WorkItems/IService.cs
Application/WorkItems/Service.cs
```

Không xóa repository và Unit of Work; chúng vẫn là dependency của handlers/behaviors.

## 21. Checkpoint 12 — Unit test pipeline và handler

Tạo test tập trung vào từng lớp trách nhiệm.

### Handler tests

- Query handler gọi đúng repository method và map đúng response.
- Create handler tạo entity `Pending` và add vào repository.
- Change status handler dùng domain method và tạo history.
- Not found trả error đúng code.
- Invalid transition trả conflict result.
- CancellationToken được truyền xuống repository.

### Validation behavior tests

- Không có validator thì gọi `next()` đúng một lần.
- Valid request gọi `next()` đúng một lần.
- Invalid request không gọi `next()`.
- Nhiều validation failure được gom đúng.

### Transaction behavior tests

- Query không gọi Begin/Commit/Rollback.
- Command success gọi Begin rồi Complete/Commit đúng thứ tự.
- Command failure result rollback.
- Handler exception rollback và exception vẫn được throw.
- Cancellation rollback và cancellation không bị đổi thành lỗi khác.

### Pipeline order test

Dùng một danh sách probe trong test để ghi thứ tự:

```text
validation-before
logging-before
transaction-before
handler
transaction-after
logging-after
validation-after
```

Không phụ thuộc hoàn toàn vào unit test mock. Integration test vẫn cần để chứng minh registration và assembly scanning đúng.

## 22. Checkpoint 13 — Integration test end-to-end

Giữ Testcontainers PostgreSQL từ Lab 2.

Test qua HTTP thật trong test host:

```text
HttpClient
→ Carter
→ ISender
→ behaviors
→ handler
→ EF Core/PostgreSQL
→ HTTP response
```

Các test bắt buộc:

1. `POST` hợp lệ trả `201` và lưu database.
2. `POST` title rỗng trả `422`; database không có row mới.
3. `GET` list chạy thành công và không cần transaction ghi.
4. `GET` ID không tồn tại trả `404` với error code đúng.
5. Update hợp lệ thay đổi status và tạo đúng một history.
6. Invalid transition trả `409`, không thêm history.
7. Stale version trả `409`, không ghi đè dữ liệu.
8. Exception giữa command rollback cả WorkItem và history.
9. CancellationToken được truyền qua sender, handler và persistence operation.
10. Ít nhất một test chứng minh handler được tìm bằng assembly scanning, không đăng ký thủ công.

Không xóa các test concurrency của Lab 2. CQRS/MediatR không thay thế cơ chế optimistic concurrency.

## 23. Required failure experiments

Thực hiện và ghi kết quả trong README:

### Experiment A — Thiếu handler

- Scan sai assembly hoặc tạm bỏ handler.
- Gửi request.
- Ghi lại loại lỗi và nguyên nhân.

### Experiment B — Sai thứ tự behavior

- Đặt transaction trước validation.
- Gửi invalid command.
- Quan sát transaction vẫn bị mở.
- Khôi phục validation đứng trước transaction.

### Experiment C — Quên gọi next

- Trong logging behavior, tạm không gọi `next()`.
- Quan sát handler không chạy.
- Giải thích behavior có quyền short-circuit pipeline.

### Experiment D — Exception trong command

- Ném exception sau thay đổi đầu tiên.
- Xác nhận rollback và exception middleware trả Problem Details.

### Experiment E — Query bị đánh dấu nhầm là command

- Cho query implement nhầm marker command.
- Quan sát transaction thừa.
- Khôi phục đúng abstraction.

## 24. Definition of done

Lab hoàn thành khi:

- `dotnet build` thành công.
- Toàn bộ unit test và integration test thành công.
- Năm use case đều đi qua MediatR.
- Endpoint chỉ inject `ISender`, không inject application service cũ.
- Handler được đăng ký bằng assembly scanning.
- Command và query có abstraction rõ ràng.
- Validation chạy bằng behavior và invalid input không chạy handler.
- Query không mở transaction.
- Command success commit; failure/exception rollback.
- Pipeline order có test hoặc bằng chứng log rõ ràng.
- CancellationToken đi xuyên suốt từ endpoint tới EF Core.
- Optimistic concurrency và PostgreSQL tests từ Lab 2 vẫn hoạt động.
- Swagger vẫn mô tả đúng success/error response.
- Không có secret thật trong repository.
- README mô tả request flow và thứ tự behaviors.

## 25. Proof of understanding

Trước khi yêu cầu review, tự trả lời trong README:

1. CQRS trong lab này giải quyết vấn đề gì? Nó không giải quyết vấn đề gì?
2. MediatR khác DI service thông thường ở điểm nào?
3. `ISender.Send()` tìm đúng handler bằng cách nào?
4. Vì sao một request chỉ nên có một handler?
5. `IPipelineBehavior` hoạt động giống middleware ở điểm nào và khác ở điểm nào?
6. Vì sao validation phải đứng trước transaction?
7. Điều gì xảy ra nếu behavior không gọi `next()`?
8. Vì sao query không nên mở explicit transaction trong lab này?
9. Business rule cần database nên nằm ở validator hay handler/domain?
10. Vì sao handler không nên trả `IResult` hoặc HTTP status code?
11. Result pattern có ưu/nhược điểm gì so với exception?
12. Vì sao command không nên được retry tự động khi chưa có idempotency?
13. Correlation ID middleware và MediatR logging behavior bổ sung cho nhau thế nào?
14. Thứ tự đăng ký behaviors ảnh hưởng runtime ra sao?
15. Nếu handler gọi một command khác qua `ISender`, transaction boundary có thể gặp vấn đề gì?

## 26. So sánh lại với production

Sau khi lab chạy ổn định, đối chiếu:

1. Message abstractions của lab với:
   `src/bmt-be.contract/abstractions/messages`.
2. Registration/ordering của lab với:
   `AddMediatRApplication()` trong Application DI extension.
3. Validation behavior của lab với:
   `ValidationPipelineBehavior<TRequest,TResponse>`.
4. Transaction behavior của lab với:
   `TransactionPipelineBehavior<TRequest,TResponse>`.
5. Timing/logging của lab với:
   `PerformancePipelineBehavior` và `TracingPipelineBehavior`.
6. Carter endpoint dùng `ISender` của lab với một endpoint production.
7. Một command handler và một query handler production để so sánh trách nhiệm.

Khi so sánh, phân loại mỗi khác biệt:

- Convention của project.
- Lựa chọn có chủ đích vì nghiệp vụ production.
- Phần lab đã đơn giản hóa.
- Điểm có thể cải thiện, không mặc định coi production là chuẩn tuyệt đối.

Đặc biệt lưu ý: source production hiện thể hiện version MediatR không đồng nhất giữa một số project. Lab chủ động dùng cùng version `12.4.0`; không học theo sự lệch version đó.

## 27. Non-goals

Chưa làm trong Mini-project 03:

- Authentication và authorization.
- Redis cache và distributed lock.
- SignalR.
- MassTransit/message broker.
- Quartz/background jobs.
- Outbox/inbox.
- Domain events hoặc MediatR notifications.
- Event sourcing.
- Separate read database/write database.
- AutoMapper.
- Retry command.
- Generic repository.

Các nội dung này được giữ cho lab sau để không làm loãng CQRS, MediatR và pipeline behaviors.

## 28. Cách làm việc với mentor

Thực hiện từng checkpoint theo thứ tự.

Với mỗi checkpoint:

1. Đọc mục tiêu và tự đề xuất file cần tạo/sửa.
2. Hỏi mentor phần chưa hiểu trước khi copy code.
3. Tự implement.
4. Build hoặc chạy nhóm test nhỏ liên quan.
5. Báo kết quả và lỗi thực tế.
6. Chỉ chuyển checkpoint khi giải thích được cơ chế vừa làm.

Mentor sẽ ưu tiên:

- Giải thích cơ chế và dependency flow.
- Render đoạn code nhỏ khi bạn yêu cầu cụ thể.
- Review implementation sau mỗi cụm checkpoint.
- Không tự động viết toàn bộ lab thay bạn.

Checkpoint bắt đầu:

```text
Checkpoint 1 — Message abstractions
```

Tiêu chí đầu tiên: bạn phải tự giải thích được quan hệ giữa `ICommand<T>`, `IRequest<Result<T>>`, handler và `ISender` trước khi chuyển sang khai báo các command/query thật.
