# 开发指南

遵循通用开发规范 [docs/DEV-GUIDE.md](docs/DEV-GUIDE.md)。
本文补充本项目的特定约束，冲突时以本文为准。

## 核心要求

- 以 Unity 游戏开发最佳实践为目标，编写简洁、高效、健壮的代码
- 充分理解用户的目标和意图，从更高、更广、更长远的角度思考事情的本质
- 使用简体中文回复；保持建设性辩证：围绕意图，给出关键假设、风险与替代思路，服务于推进而非反驳

## 工程边界

- 基于 Unity 2022.3 LTS，最高使用 C# 10，Nullable 关闭

## 代码规范

- 严格遵循 [.editorconfig](.editorconfig)：C# 文件使用 LF、TAB 缩进，宽度为 4
- 异步优先使用 UniTask 与 `async`/`await`
- 类型简写优先使用 `var` 和 `new()`
- 单例使用 `ArkSharp.Singleton.Get<T>()`
- 日志使用 `ArkSharp.Log.Info/Warn/Error`
- 命名规范：
  - 常量、只读字段 `FULL_UPPER_CASE`
  - 类型、方法、公共成员 `PascalCase`
  - 私有字段 `_camelCase`（下划线前缀）
  - 私有属性 `camelCase`
  - 接口 `IService`（I 前缀）

## 单元测试

- 使用 NUnit 和经典模式 `Assert` 语法
- 测试类名、文件名以 `Test` 开头
- 异步测试使用 `public async Task TestMethod()`
- 异步异常测试应直接 `await`，以 `try`/`catch` 捕获异常，再用 `Assert.IsInstanceOf<T>` 验证类型。
- 禁止使用 `Assert.ThrowsAsync`，避免其同步等待阻塞 Unity 主线程导致死锁。
- 测试须独立、可重复，不修改业务代码迁就测试
