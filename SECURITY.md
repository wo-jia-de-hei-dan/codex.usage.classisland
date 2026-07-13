# 安全说明

- 插件只访问固定的 `https://chatgpt.com/backend-api/wham/usage` 地址。
- auth 文件中的 token 只在内存中用于本次请求，不写入插件配置、日志或打包文件。
- 错误消息只包含状态类别或 HTTP 状态码，不包含请求头和响应原文。
- `.cipx` 包不应与 auth 文件、浏览器 cookies 或任何个人凭据放在同一个分享压缩包中。
# 安全与隐私说明

- 不要提交 `.codex/auth.json`、Token、API Key、日志或 `settings.json`。
- 本插件只在本机读取 Codex 登录状态，并请求用量接口。
- 请在公开发布前检查 Git 历史、压缩包内容和构建输出。
- 如果怀疑凭据泄露，请立即在对应服务中撤销并重新登录。
