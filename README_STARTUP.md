# KnewFate 项目启动指南

## 项目概述
KnewFate 是一个基于 .NET MAUI 8.0 的跨平台命理计算应用，支持：
- 多语言同步（中文/英文）
- AI客户服务系统  
- 用户认证系统
- 八字、占星、塔罗等多种命理系统

## 启动前准备

### 1. 安装 .NET SDK
由于检测到您的系统缺少 .NET SDK，请按以下步骤安装：

1. 访问 https://dotnet.microsoft.com/download/dotnet/8.0
2. 下载并安装 .NET 8.0 SDK (推荐 LTS 版本)
3. 重启 Visual Studio

### 2. 安装 MAUI 工作负载
在安装 .NET SDK 后，打开命令提示符（以管理员身份）运行：
```
dotnet workload install maui
```

### 3. Visual Studio 配置
确保 Visual Studio 2022 安装了以下组件：
- .NET Multi-platform App UI development
- .NET desktop development
- Mobile development with .NET

## 启动步骤

### 方法一：使用 Visual Studio（推荐）
1. 双击打开 `KnewFate.sln` 文件
2. 在 Visual Studio 中选择目标平台：
   - Windows Machine (本地测试)
   - Android Emulator
   - iOS Simulator (需要 Mac)
3. 点击"开始调试"按钮 (F5)

### 方法二：使用命令行
```powershell
# 进入项目目录
cd "c:\Users\Yixin Zhang\Desktop\knew_fate"

# 恢复 NuGet 包
dotnet restore

# 构建项目
dotnet build

# 运行 Windows 版本
dotnet run --framework net8.0-windows10.0.19041.0
```

## 项目特性

### 已实现功能
✅ 多语言本地化系统
✅ AI客户服务浮动助手  
✅ 用户认证系统（登录/注册/忘记密码）
✅ 现代化UI设计
✅ SQLite数据库集成
✅ 依赖注入配置

### 主要页面
- 登录页面：`Views/LoginPage.xaml`
- 注册页面：`Views/RegisterPage.xaml`  
- 忘记密码：`Views/ForgotPasswordPage.xaml`
- AI客服管理：`Views/AICustomerServiceDashboard.xaml`
- 设置页面：`Views/SettingsPage.xaml`

### 认证流程
1. 应用启动时检查用户登录状态
2. 未登录用户自动跳转到登录页面
3. 支持用户注册、密码重置
4. 安全的密码哈希存储

## 故障排除

### 常见问题
1. **"dotnet 不是内部或外部命令"**
   - 安装 .NET 8.0 SDK
   - 重启终端/Visual Studio

2. **NuGet 包还原失败**
   - 检查网络连接
   - 运行 `dotnet nuget locals all --clear`

3. **MAUI 工作负载缺失**
   - 运行 `dotnet workload install maui`

4. **Android 模拟器问题**
   - 确保安装了 Android SDK
   - 检查 Hyper-V 设置

## 下一步开发

### 待完成功能
- [ ] 数据库迁移脚本
- [ ] 八字计算引擎
- [ ] 占星图表生成
- [ ] AI分析报告
- [ ] 社交分享功能

### 建议的开发顺序
1. 完成基本认证流程测试
2. 实现用户资料管理
3. 添加命理计算核心功能
4. 集成AI分析服务
5. 完善UI/UX设计

## 技术架构

### 核心技术栈
- **.NET MAUI 8.0**: 跨平台UI框架
- **SQLite**: 本地数据库
- **CommunityToolkit.Maui**: UI组件库
- **Microsoft.Extensions**: 依赖注入和本地化

### 项目结构
```
KnewFate/
├── Models/           # 数据模型
├── Services/         # 业务服务
├── Views/           # UI页面
├── ViewModels/      # MVVM视图模型
├── Resources/       # 资源文件
└── Platforms/       # 平台特定代码
```

## 联系支持
如果在启动过程中遇到问题，请检查：
1. Visual Studio 输出窗口的错误信息
2. NuGet 包管理器的还原状态
3. 项目属性中的目标框架设置

祝您成功启动 KnewFate 项目！🎉
