# 免费重置额度显示 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 ClassIsland Codex 使用量插件中增加独立的免费重置额度展示，不改变现有提醒和自动化规则。

**Architecture:** 扩展现有 `UsageResponseParser` 与 `UsageSnapshot`，将免费额度作为可选数据传递到 `UsageService` 和 `CodexUsageComponent`。免费额度只进入展示链路，不进入 `UsageNotificationService`、`CodexUsageLowTrigger` 或设置持久化。

**Tech Stack:** .NET 8、C#、Avalonia/ClassIsland 插件 API、现有控制台测试项目。

## Global Constraints

- 免费额度缺失或格式异常时必须显示“暂未提供”，不能显示为 0%。
- 免费额度与每周额度必须独立建模、独立展示。
- 本次不新增免费额度阈值、提醒或自动化规则。
- 不保存完整响应、Token、API Key 或本机认证内容。
- 保持现有提醒、自动化、设置保存和非渐变图标行为不变。

## 文件结构

- Modify: `UsageSnapshot.cs` — 增加可选免费额度字段。
- Modify: `UsageResponseParser.cs` — 解析免费重置额度节点，兼容缺失和异常字段。
- Create: `UsageDisplayFormatter.cs` — 提供不依赖 Avalonia 的展示文本格式化。
- Modify: `CodexUsageComponent.cs` — 增加免费额度卡片的展示属性和布局。
- Modify: `../codex-usage-classisland.Tests/Program.cs` — 增加解析回归测试。
- Modify: `README.md` — 更新功能说明，明确免费额度只展示、不参与提醒。
- Create: `docs/superpowers/plans/2026-07-14-free-reset-display.md` — 本计划。

### Task 1: 为免费额度解析定义失败测试

**Files:**
- Modify: `../codex-usage-classisland.Tests/Program.cs`
- Test target: `UsageResponseParser.Parse`

**Interfaces:**
- Consumes: 现有 JSON 字符串和 `UsageResponseParser.Parse(string)`。
- Produces: 明确的免费额度字段命名和缺失行为，供生产代码实现。

- [ ] **Step 1: 写入失败测试**

添加三个测试用例：对解析器使用现有 JSON 结构，在 `usage` 对象中加入 `free_reset` 对象，内部使用 `remaining_percent` 和 `reset_at`；断言该节点存在时可解析剩余百分比和重置时间；节点缺失时断言每周额度仍成功且免费额度为 null；免费字段格式异常时断言每周额度仍成功且免费额度为 null。

测试断言使用 `snapshot.FreeResetUsage is not null`、`snapshot.FreeResetUsage!.RemainingPercent` 和 `snapshot.FreeResetUsage is null`，不要把完整响应写入输出。

- [ ] **Step 2: 运行测试确认按预期失败**

Run:

```powershell
dotnet run --project ..\codex-usage-classisland.Tests\CodexUsageClassIsland.Tests.csproj -c Release
```

Expected: 编译或断言失败，原因是 `FreeResetUsage` 尚不存在；现有 11 个测试的行为不能被删除。

- [ ] **Step 3: Commit**

```powershell
git add ..\codex-usage-classisland.Tests\Program.cs
git commit -m "test: define free reset usage parsing"
```

### Task 2: 扩展快照和解析器

**Files:**
- Modify: `UsageSnapshot.cs`
- Modify: `UsageResponseParser.cs`
- Modify: `UsageDisplayFormatter.cs`

**Interfaces:**
- Consumes: Codex 使用量 JSON。
- Produces: `UsageSnapshot.FreeResetUsage`，类型与现有额度明细一致且允许为 null。

- [ ] **Step 1: 写入最小数据类型**

沿用现有每周额度的不可变额度类型，增加可选的免费额度字段；免费额度至少包含 `RemainingPercent` 和 `ResetAt`，并保持百分比为 0–100 的整数或现有项目使用的同等数值类型。

- [ ] **Step 2: 实现最小解析逻辑**

在 `UsageResponseParser.Parse` 中递归查找 `free_reset` 对象，并在该对象内读取 `remaining_percent` 与 `reset_at`；节点不存在、不是对象、百分比无法转换或超出有效范围时返回 null。任何免费字段异常不得影响每周额度解析。

- [ ] **Step 3: 运行定向测试确认通过**

Run:

```powershell
dotnet run --project ..\codex-usage-classisland.Tests\CodexUsageClassIsland.Tests.csproj -c Release
```

Expected: 新增免费额度测试及原有测试全部通过，输出仍为项目既有 PASS 格式。

- [ ] **Step 4: Commit**

```powershell
git add UsageSnapshot.cs UsageResponseParser.cs ..\codex-usage-classisland.Tests\Program.cs
git commit -m "feat: parse optional free reset usage"
```

### Task 3: 增加组件展示

**Files:**
- Modify: `CodexUsageComponent.cs`

**Interfaces:**
- Consumes: `UsageService` 发布的 `UsageSnapshot`。
- Produces: 免费额度标题、百分比、进度值和重置时间的绑定属性。

- [ ] **Step 1: 写入组件显示测试或可验证的展示断言**

在 `../codex-usage-classisland.Tests/Program.cs` 中增加组件使用的纯格式化辅助方法测试：有效免费额度输出“免费重置额度”和百分比；null 免费额度输出“暂未提供”；每周额度字段仍按原格式输出。该测试不创建 Avalonia 控件。

- [ ] **Step 2: 运行测试确认失败**

Run:

```powershell
dotnet run --project ..\codex-usage-classisland.Tests\CodexUsageClassIsland.Tests.csproj -c Release
```

Expected: 组件格式化 API 或免费额度属性不存在导致失败。

- [ ] **Step 3: 实现最小展示变化**

在现有组件布局中增加一张与每周额度同级的独立卡片，使用现有颜色、间距和控件样式，不引入蓝紫渐变。免费额度不可用时显示“暂未提供”，不显示误导性的空进度条或 0%。不修改现有刷新、提醒和自动化调用。

- [ ] **Step 4: 运行测试确认通过**

Run:

```powershell
dotnet run --project ..\codex-usage-classisland.Tests\CodexUsageClassIsland.Tests.csproj -c Release
```

Expected: 展示断言与全部既有测试通过。

- [ ] **Step 5: Commit**

```powershell
git add CodexUsageComponent.cs ..\codex-usage-classisland.Tests\Program.cs
git commit -m "feat: show free reset usage card"
```

### Task 4: 更新文档、构建并生成插件包

**Files:**
- Modify: `README.md`
- Generated: `bin/`, `obj/`, `cipx/` and `../CodexUsageClassIsland-0.1.5.cipx`

**Interfaces:**
- Consumes: 已通过测试的插件项目。
- Produces: 可安装的新版 `.cipx`，不包含 `auth.json`、Token、日志或本机配置。

- [ ] **Step 1: 更新 README**

说明组件现在同时显示每周额度和可用的免费重置额度；明确免费额度仅用于显示，本版本不参与提醒和自动化。

- [ ] **Step 2: 执行完整测试**

```powershell
dotnet run --project ..\codex-usage-classisland.Tests\CodexUsageClassIsland.Tests.csproj -c Release
```

Expected: 全部测试通过，0 个失败。

- [ ] **Step 3: 执行 Release 构建**

```powershell
dotnet build .\CodexUsageClassIsland.csproj -c Release --no-restore
```

Expected: exit code 0，0 errors，0 warnings。

- [ ] **Step 4: 重建 `.cipx` 并扫描敏感信息**

从 `bin\Release\net8.0-windows` 取出构建产物，按现有 `cipx\CodexUsageClassIsland.cipx` 的目录结构生成 ZIP，并保存为 `..\CodexUsageClassIsland-0.1.5.cipx`；使用 `System.IO.Compression.ZipFile` 列出包内文件，再扫描 README、manifest 和文本元数据，确认不存在 API Key 值、Token 值、`auth.json`、日志或本机绝对路径。更新 `cipx\checksums.md` 中的 MD5 后再核对包文件存在。

- [ ] **Step 5: Commit**

```powershell
git add README.md
git commit -m "docs: describe free reset usage display"
```
