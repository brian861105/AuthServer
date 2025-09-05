# AuthServer 開發待辦清單

## 已完成 ✅
- [x] 基本認證 API（註冊、登入、忘記密碼、重設密碼）
- [x] Clean Architecture 分層架構
- [x] InMemoryUserRepository 實作
- [x] Docker 容器化配置
- [x] 基本單元測試框架
- [x] 添加 JWT Token 生成和驗證 (`JwtTokenService`)
- [x] 實作受保護的端點 JWT Token 驗證示範端點 (`/api/auth/validate-token`)
- [x] 添加用戶資料驗證（Email 格式、密碼強度）
- [x] 添加健康檢查端點 (`/health`)
- [x] JWT 驗證集成測試
- [x] 代碼格式化和 lint 檢查
- [x] 測試覆蓋率收集和 HTML 報告生成
- [x] 測試和 lint 腳本 (`test.sh`, `lint.sh`)
- [x] **重構錯誤處理機制**：
  - [x] 移除 Result Pattern，改用例外處理
  - [x] 添加 Problem Details 中介軟體
  - [x] 簡化 Controller 錯誤處理邏輯
  - [x] 統一錯誤回應格式（RFC 7807 標準）
  - [x] 更新單元測試以適應新的例外處理機制

## 進行中 🚧

### Phase 1: 完善 InMemory 版本
- [x] 添加 JWT Token 生成和驗證
- [x] 實作受保護的端點（需要認證）
- [x] 添加用戶資料驗證（Email 格式、密碼強度）
- [x] 完善錯誤處理和回應格式（已重構為例外處理 + Problem Details）
- [ ] 添加 API 文檔（Swagger 優化）
- [x] 添加健康檢查端點 `/health`

### Phase 2: EF Core 資料庫整合
- [ ] 設計 EF Core DbContext
- [ ] 建立 Database Migrations
- [ ] 實作 EfUserRepository
- [ ] 設定資料庫連線配置（開發/生產環境）
- [ ] 資料庫索引優化（Email 唯一索引）
- [ ] 資料庫連線池設定

### Phase 3: AOT 最佳化
- [ ] 移除 InMemoryUserRepository 的反射程式碼
- [ ] 配置 EF Core 支援 AOT 編譯
- [ ] 添加 JSON 序列化 Source Generator
- [ ] 測試 AOT 編譯相容性
- [ ] 更新 Dockerfile 支援 AOT
- [ ] 效能基準測試（AOT vs JIT）

### Phase 4: 安全性強化
- [ ] 實作帳號鎖定機制（防暴力破解）
- [ ] 添加 Rate Limiting
- [ ] 實作 CORS 設定
- [ ] 添加 HTTPS 重導向
- [ ] 敏感資料加密（如重設密碼 Token）
- [ ] 安全標頭設定

### Phase 5: 監控和日誌
- [ ] 結構化日誌（Serilog）
- [ ] 應用程式指標收集
- [ ] 錯誤追蹤和警報
- [ ] 效能監控
- [ ] 審計日誌（登入、密碼變更等重要操作）

### Phase 6: 測試完善
- [x] 完善單元測試覆蓋率（已有 HTML 覆蓋率報告）
- [x] 添加整合測試（JWT 驗證測試）
- [x] API 端點測試（AuthController 測試）
- [x] 更新單元測試以適應例外處理機制：
  - [x] `AuthService` 測試按函數分組重新組織
  - [x] 更新為使用 `Assert.ThrowsAsync<T>()` 測試例外
  - [x] 涵蓋 `ArgumentException`, `InvalidOperationException`, `UnauthorizedAccessException`
  - [x] 增加邊界條件和安全考量測試
- [ ] 效能測試
- [ ] 安全性測試

### Phase 7: 部署和維運
- [ ] CI/CD 管道設定
- [ ] Docker Compose 生產環境配置
- [ ] Kubernetes 部署 YAML
- [ ] 備份和還原策略
- [ ] 監控和警報設定

## 當前狀態 📊

### 測試覆蓋率
- **AuthServer.Infrastructure**: 73%
  - `InMemoryUserRepository`: 63.8%
  - `JwtTokenService`: 87%
- **AuthServer.Domain.ValidationResult**: 77.7%

### 可用腳本
```bash
./test.sh   # 運行測試並生成 HTML 覆蓋率報告
./lint.sh   # 檢查代碼格式
```

### 已實現 API 端點
- `POST /api/auth/register` - 用戶註冊
- `POST /api/auth/login` - 用戶登入  
- `POST /api/auth/forgot-password` - 忘記密碼
- `POST /api/auth/reset-password` - 重設密碼
- `GET /api/auth/validate-token` - JWT Token 驗證 (需要授權)
- `GET /health` - 健康檢查

## 技術債務 📋
- [ ] InMemoryUserRepository 使用反射設定 ID（將被 EF Core 取代）
- [x] ~~缺少適當的例外處理策略~~（已重構為統一例外處理）
- [ ] 密碼重設 Token 沒有過期時間
- [ ] 缺少 API 版本控制
- [x] ~~需要更新單元測試以適應新的例外處理機制~~（已完成）

## 架構決策記錄 📝
- **資料存取**: 開發階段使用 InMemory，生產環境使用 EF Core + SQL Server
- **認證方式**: JWT Token
- **錯誤處理**: 例外處理 + Problem Details 中介軟體（RFC 7807 標準）
- **容器化**: Docker + 未來支援 AOT 編譯
- **測試策略**: NUnit + 整合測試
- **日誌框架**: ASP.NET Core 內建 + 未來考慮 Serilog

## 注意事項 ⚠️
- InMemory 實作僅供開發測試，不適用於生產環境
- AOT 支援需要等到 EF Core 實作完成後再進行
- 所有敏感資料（密碼、Token）必須適當加密和保護
- API 設計需考慮向後相容性