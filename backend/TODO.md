# AuthServer 測試指南

## 前置條件

### 安裝 .NET 8 SDK
```bash
# macOS (使用 Homebrew)
brew install --cask dotnet

# 或從官網下載
https://dotnet.microsoft.com/download/dotnet/8.0
```

### 驗證安裝
```bash
dotnet --version
# 應該顯示 8.x.x
```

## 建置專案

### 建置整個解決方案
```bash
cd /Users/youchen/Documents/AuthServer/backend
dotnet build
```

### 運行單元測試
```bash
# 運行所有測試
dotnet test

# 運行特定測試專案
dotnet test tests/AuthServer.Infrastructure.Tests/
dotnet test tests/AuthServer.Application.Tests/
```

## 運行 API

### 啟動 AuthServer
```bash
cd src/API
dotnet run
```

### 訪問 Swagger UI
```
https://localhost:7xxx/swagger
http://localhost:5xxx/swagger
```
(實際 port 會顯示在命令列輸出)

## API 測試

### 1. 註冊用戶
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "password123"
}
```

### 2. 登入
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "password123"
}
```

### 3. 忘記密碼
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}
```
*注意：重設連結會顯示在 console 輸出*

### 4. 重設密碼
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "token": "從 console 複製的 token",
  "newPassword": "newpassword123"
}
```

## 預期結果

### 註冊成功
```json
{
  "token": "base64編碼的簡單token"
}
```

### 登入成功
```json
{
  "token": "base64編碼的簡單token"
}
```

### 忘記密碼成功
```json
{
  "message": "Password reset link sent to your email"
}
```

### 重設密碼成功
```json
{
  "message": "Password reset successfully"
}
```

## 故障排除

### 常見問題

1. **dotnet: command not found**
   - 確保已安裝 .NET 8 SDK
   - 重新啟動終端機

2. **建置錯誤**
   - 檢查所有專案檔案是否正確
   - 運行 `dotnet restore` 還原套件

3. **測試失敗**
   - 檢查 BCrypt.Net-Next 套件是否正確安裝
   - 確認測試專案參考正確

4. **API 啟動失敗**
   - 檢查 port 是否被佔用
   - 確認所有依賴注入設定正確

## 檔案檢查清單

確保以下檔案存在且內容正確：

### Domain Layer
- [x] `src/Domain/Entities/User.cs`
- [x] `src/Domain/Common/Result.cs`
- [x] `src/Domain/Interfaces/IUserRepository.cs`
- [x] `src/Domain/Interfaces/IAuthService.cs`

### Application Layer
- [x] `src/Application/Services/AuthService.cs`
- [x] `src/Application/DTOs/RegisterRequest.cs`
- [x] `src/Application/DTOs/LoginRequest.cs`
- [x] `src/Application/DTOs/AuthResponse.cs`
- [x] `src/Application/DTOs/ForgotPasswordRequest.cs`
- [x] `src/Application/DTOs/ResetPasswordRequest.cs`

### Infrastructure Layer
- [x] `src/Infrastructure/Data/InMemoryUserRepository.cs`

### API Layer
- [x] `src/API/Controllers/AuthController.cs`
- [x] `src/API/Program.cs`

### Tests
- [x] `tests/AuthServer.Infrastructure.Tests/Data/InMemoryUserRepositoryTests.cs`
- [x] `tests/AuthServer.Application.Tests/Services/AuthServiceTests.cs`

### Project Files
- [x] `AuthServer.sln`
- [x] `src/Domain/AuthServer.Domain.csproj`
- [x] `src/Application/AuthServer.Application.csproj`
- [x] `src/Infrastructure/AuthServer.Infrastructure.csproj`
- [x] `src/API/AuthServer.API.csproj`
- [x] `tests/AuthServer.Infrastructure.Tests/AuthServer.Infrastructure.Tests.csproj`
- [x] `tests/AuthServer.Application.Tests/AuthServer.Application.Tests.csproj`

## 下一步擴展

- [ ] 實現真實的 JWT Token 生成
- [ ] 加入資料庫持久化 (Entity Framework)
- [ ] 實現 Email 發送功能
- [ ] 加入輸入驗證
- [ ] 實現 Refresh Token
- [ ] 加入 API 認證中介軟體
- [ ] 部署至雲端平台