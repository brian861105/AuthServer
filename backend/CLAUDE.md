# C# AuthServer 開發指南

## 核心架構

### 分層架構
```
Controllers (API) → Use Cases (Application) → Entities (Domain) → Database (Infrastructure)
```

### 專案結構
```
backend/
├── src/
│   ├── Domain/         # 實體、值物件、介面
│   ├── Application/    # 用例、服務
│   ├── Infrastructure/ # 資料庫、外部服務
│   └── API/           # 控制器、DTO
└── tests/
```

## 開發原則

### SOLID 原則

#### S - Single Responsibility Principle (單一職責原則)
```csharp
// ✅ 好的設計 - 每個類別只有一個責任
public class AuthService : IAuthService
{
    // 只負責認證相關業務邏輯
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request) { }
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request) { }
}

public class UserRepository : IUserRepository
{
    // 只負責使用者資料存取
    public async Task<User?> GetByEmailAsync(string email) { }
    public async Task<User> AddAsync(User user) { }
}

public class PasswordService : IPasswordService
{
    // 只負責密碼相關操作
    public string HashPassword(string password) { }
    public bool VerifyPassword(string password, string hash) { }
}
```

#### O - Open/Closed Principle (開放封閉原則)
```csharp
// ✅ 對擴展開放，對修改封閉
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}

// 可以擴展不同的實現，不需要修改原有程式碼
public class InMemoryUserRepository : IUserRepository { }
public class SqlUserRepository : IUserRepository { }
public class MongoUserRepository : IUserRepository { }
```

#### L - Liskov Substitution Principle (里氏替換原則)
```csharp
// ✅ 子類別可以替換父類別而不影響程式正確性
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
}

// 任何 IUserRepository 的實現都應該能正常工作
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository; // 可以是任何實現
    
    public AuthService(IUserRepository repository)
    {
        _repository = repository; // 依賴抽象，不依賴具體實現
    }
}
```

#### I - Interface Segregation Principle (介面隔離原則)
```csharp
// ✅ 將大的介面分割成更小、更專用的介面
public interface IUserReader
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
}

public interface IUserWriter
{
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}

// 服務只依賴它需要的介面
public class AuthService : IAuthService
{
    private readonly IUserReader _userReader;
    private readonly IUserWriter _userWriter;
    
    public AuthService(IUserReader userReader, IUserWriter userWriter)
    {
        _userReader = userReader;
        _userWriter = userWriter;
    }
}

// 但在曳光彈階段，可以合併介面簡化開發
public interface IUserRepository : IUserReader, IUserWriter
{
    // 繼承兩個介面，提供完整功能
}
```

#### D - Dependency Inversion Principle (依賴倒置原則)
```csharp
// ✅ 高層模組不應該依賴低層模組，都應該依賴抽象
// 高層模組 (Application Layer)
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository; // 依賴抽象
    private readonly IPasswordService _passwordService; // 依賴抽象
    
    public AuthService(IUserRepository repository, IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }
}

// 低層模組 (Infrastructure Layer) 實現抽象
public class InMemoryUserRepository : IUserRepository { }
public class BCryptPasswordService : IPasswordService { }

// 依賴注入配置
services.AddScoped<IUserRepository, InMemoryUserRepository>();
services.AddScoped<IPasswordService, BCryptPasswordService>();
services.AddScoped<IAuthService, AuthService>();
```

### 依賴倒置原則 - Interface First
```csharp
// 1. 先定義介面 (Domain Layer)
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User> AddAsync(User user);
}

public interface IAuthService
{
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
}

// 2. 再實現服務 (Application Layer)
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository; // 依賴抽象
    
    public AuthService(IUserRepository repository)
    {
        _repository = repository;
    }
}

// 3. 最後實現具體類別 (Infrastructure Layer)
public class InMemoryUserRepository : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) { ... }
}

// 4. 依賴注入配置
services.AddScoped<IUserRepository, InMemoryUserRepository>();
services.AddScoped<IAuthService, AuthService>();
```

### 命名
```csharp
// ✅ 好的命名
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request) { }
}

// ❌ 避免
public class AS { var r; public Task<Result<User>> Reg(Req r) { } }
```

### 實體設計
```csharp
public class User : Entity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private User() { } // ORM
    
    public User(string email, string passwordHash)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        CreatedAt = DateTime.UtcNow;
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }
}
```


### 🚨 TDD 驅動開發 - 最高優先級原則 ⚠️ **必須遵循**

🔴 **RED → 🟢 GREEN → 🔵 REFACTOR** - 這是鐵律！

#### 核心原則
- **🔴 RED**: 先寫失敗測試再寫實作（沒有例外！）
- **🟢 GREEN**: 只寫讓測試通過的最小代碼
- **🔵 REFACTOR**: 測試通過後才能重構
- **📝 DOCUMENT**: 測試即規格，測試即文檔
- **⚡ FAST**: 快速的反饋循環（秒級執行）

#### ⛔ TDD 禁止事項
- ❌ 沒有測試就寫產品代碼
- ❌ 寫通過的測試（測試必須先失敗）
- ❌ 跳過重構階段
- ❌ 寫太複雜的測試

#### 實際開發流程

```csharp
// 步驟 1: 先寫失敗的測試 (紅燈)
[Test]
public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
{
    // Arrange
    var authService = new AuthService(mockRepository);
    var request = new LoginRequest("test@example.com", "password");
    
    // Act
    var result = await authService.LoginAsync(request);
    
    // Assert
    Assert.That(result.IsSuccess, Is.True);
    Assert.That(result.Value.Token, Is.Not.Empty);
}

// 步驟 2: Domain Layer - 定義介面讓測試編譯通過
public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
}

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByResetTokenAsync(string token);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
}

// 步驟 3: 實現最小實作讓測試通過 (綠燈)
public class AuthService : IAuthService
{
    private readonly IUserRepository _repository;
    
    public AuthService(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        // 最小實作 - 硬編碼讓測試通過
        if (request.Email == "test@example.com" && request.Password == "password")
        {
            return Result<AuthResponse>.Success(new AuthResponse("fake-token"));
        }
        return Result<AuthResponse>.Failure("Invalid credentials");
    }
}

// 步驟 4: 重構和完善 (藍燈)
public class AuthService : IAuthService
{
    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _repository.GetByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthResponse>.Failure("User not found");
            
        if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            return Result<AuthResponse>.Failure("Invalid password");
            
        var token = _tokenService.GenerateToken(user);
        return Result<AuthResponse>.Success(new AuthResponse(token));
    }
}

// 步驟 5: 繼續 TDD 循環，添加更多測試
[Test]
public async Task LoginAsync_WithInvalidEmail_ShouldReturnError()
{
    // 新的紅燈測試...
}
```

### 🚨 TDD 檢查清單 - 每次開發前必須確認

- [ ] **先寫測試** - 沒有測試不能寫產品代碼
- [ ] **測試必須先失敗** - 確保測試真的在測試功能
- [ ] **只寫讓測試通過的最小代碼** - 不要過度工程
- [ ] **測試通過後才能重構** - 紅燈 → 綠燈 → 重構
- [ ] **每個功能都要有對應測試** - 測試即規格

### 開發步驟總結

```
1. 🔴 寫失敗測試 (Red)
2. 🟢 寫最小實作 (Green) 
3. 🔵 重構改善 (Refactor)
4. 重複步驟 1-3
```

## 曳光彈開發方法 (Tracer Bullet Development)

### 概念說明

曳光彈開發是一種敏捷開發方法，透過建立最小可行的端到端系統來驗證架構和核心功能，然後逐步擴展和改進。

### 實施步驟

#### 1. 識別核心用例
```csharp
// 首先實現最基本的用戶註冊流程
public class UserRegistrationTracerBullet
{
    // 最簡單的實現 - 僅驗證端到端流程
    public async Task<string> RegisterUser(string email, string password)
    {
        // 省略複雜驗證，專注於流程打通
        var userId = Guid.NewGuid().ToString();
        
        // 簡單的內存存儲
        _users.Add(new User { Id = userId, Email = email, Password = password });
        
        return userId;
    }
}
```

#### 2. 建立骨架架構
```csharp
// 建立基本的分層結構，先用最簡單的實現
public interface IUserService
{
    Task<string> RegisterUserAsync(string email, string password);
}

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    
    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<string> RegisterUserAsync(string email, string password)
    {
        // 曳光彈階段：最簡實現
        var user = new User(email, password);
        await _repository.SaveAsync(user);
        return user.Id;
    }
}
```

#### 3. 端到端驗證
```csharp
// 建立端到端測試驗證整個流程
[Test]
public async Task TracerBullet_UserRegistration_ShouldWorkEndToEnd()
{
    // Arrange - 使用真實的依賴項目或最簡單的實現
    var repository = new InMemoryUserRepository();
    var service = new UserService(repository);
    var controller = new UserController(service);
    
    // Act - 測試完整流程
    var request = new RegisterUserRequest { Email = "test@example.com", Password = "password" };
    var result = await controller.RegisterUser(request);
    
    // Assert - 驗證端到端功能
    result.Should().BeOfType<CreatedResult>();
    var user = await repository.GetByEmailAsync("test@example.com");
    user.Should().NotBeNull();
}
```

#### 4. 逐步演進
```csharp
// 第一次迭代：加入基本驗證
public async Task<Result<string>> RegisterUserAsync(string email, string password)
{
    if (string.IsNullOrEmpty(email))
        return Result<string>.Failure("Email is required");
    
    if (string.IsNullOrEmpty(password))
        return Result<string>.Failure("Password is required");
    
    var user = new User(email, password);
    await _repository.SaveAsync(user);
    return Result<string>.Success(user.Id);
}

// 第二次迭代：加入業務規則
public async Task<Result<string>> RegisterUserAsync(string email, string password)
{
    var validationResult = await ValidateUserRegistration(email, password);
    if (validationResult.IsFailure)
        return Result<string>.Failure(validationResult.Error);
    
    if (await _repository.ExistsByEmailAsync(email))
        return Result<string>.Failure("User already exists");
    
    var user = new User(email, HashPassword(password));
    await _repository.SaveAsync(user);
    await SendWelcomeEmail(user);
    
    return Result<string>.Success(user.Id);
}
```

### 曳光彈開發檢查清單

- [ ] 識別最核心的使用場景
- [ ] 建立最簡單的端到端實現
- [ ] 編寫端到端測試
- [ ] 驗證架構可行性
- [ ] 逐步加入業務規則
- [ ] 重構改進代碼品質
- [ ] 加入完整的錯誤處理
- [ ] 優化性能和安全性

## 關鍵原則總結

### Clean Architecture
- 依賴倒轉：內層不依賴外層
- 穩定抽象原則：抽象比具體更穩定
- 關注點分離：每層都有明確的責任

### Clean Code
- 有意義的命名
- 函數應該小且專一
- 減少註解，讓代碼自說明
- 錯誤處理不可忽視

### DDD
- 豐富的領域模型
- 聚合確保一致性
- 領域事件處理副作用
- 通用語言在代碼中體現


## 測試框架

本專案使用 **NUnit** 作為測試框架。

這個指南提供了一個堅實的基礎來構建可維護、可測試和可擴展的 C# 應用程序。