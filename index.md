# **Keep.Discovery**

## 简介
[Keep Discovery](https://github.com/redochenzhen/discovery)是基于asp.net core平台的服务发现框架。目前具有以下特性和限制：

### 特性：
1. 基于抽象实现，方便扩展支持其他热门服务发现组件，如Consul, Eureka等
2. 拥有可靠的容灾能力
3. 对开发完全透明，请求由标准的HttpClient完成
4. 内置了“静态发现”功能，方便本地调试
5. 可与Refit集成，只需申明接口，自动成成Client代码

### 限制（暂时）：
1. 只支持Restful Api (http协议)，不支持WCF, gRPC等rpc协议
2. 只支持基于ZooKeeper实现的ZooPicker服务发现组件

## 快速开始
### NuGet
```
dotnet add package Keep.Discovery
dotnet add package Keep.Discovery.ZooPicker
```

### ZooPicker
ZooPicker是以ZooKeeper为基础，对Keep.Discovery抽象的具体实现

### 服务提供端
### 配置
在Startup.cs中配置Discovery：
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDiscovery(options =>
    {   
        // 与配置"ShouldRegister": true等效，二选一即可
        options.ShouldRegister = true;
        options.UseZooPicker();
    });
    
    ... ...
}
```
并提供正确的配置：
```json
{
  ... ...

  "Discovery": {
    "ShouldRegister": true,

    "ZooPicker": {
      "ConnectionString": "127.0.0.1:2181/services",
      "SessionTimeout": 4000,
      "ConnectionTimeout": 3000,
      "GroupName": "yourgroup",
      //"Password": "123456",

      "Instance": {
        "ServiceName": "testapi",
        "Type": "Rest",
        "Port": 5003,
        "Balancing": "RoundRobin",
        "IpAddress": "127.0.0.1",
        "PreferIpAddress": true,

        "Weight": 1,
        "State": "Up",

        "FailTimeout": 10000,
        "MaxFails": 1,
        "NextWhen": "Error,Timeout",
        "NextTries": 3,
        "NextTimeout": 0
      }
    }
  }
}
```
这样，你的服务实例http://localhost:5000将会以yourservicename为名字，注册到ZooKeeper服务器上。
这里值得注意的是，配置项Discovery:ShouldRegister被设置为true，表明该应用程序应注册服务（而不启用发现功能）。

### 服务消费端
在Startup.cs中配置Discovery：
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDiscovery(options =>
    {   
        options.ShouldDiscover = true;
        options.UseZooPicker();
    })
    .AddDiscoveryHttpClient<ITestClient, TestClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://yourservicename"));
    
    ... ...
}

public interface ITestClient
{
    Task<User> GetUserByIdAsync(int id);
}

class TestClient : ITestClient
{
    private readonly HttpClient _http;

    public TestClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<User> GetUserByIdAsync(int id)
    {
        var response = await _http.GetAsync($"/users/{id}");
        if (!response.IsSuccessStatusCode)
        {
            throw new NotSupportedException();
        }
        var user = await response.Content.ReadAsAsync<User>();
        return user;
    }
}

```
并提供正确的配置：
```json
{
  ... ...

  "Discovery": {
    "ShouldDiscover": true,
    //"PathMatch": "/mapping",

    "ZooPicker": {
      "ConnectionString": "192.168.117.52:2181/services",
      "SessionTimeout": 4000,
      "ConnectionTimeout": 3000,
      "GroupName": "yourgroup",
      //"Password": "123456",
    },

    "Mapping": []
  }
}
```

这样，即可在你的业务Service层中依赖注入TestClient的实例，来完成外部服务的调用。如果，你熟悉Refit, 可安装Keep.Discovery.Refit包，
Startup.cs的配置调整为：
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDiscovery(options =>
    {   
        options.ShouldDiscover = true;
        options.UseZooPicker();
    })
    .AddDiscoveryRefitClient<ITestRefitClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://yourservicename"));
    
    ... ...
}
```
并申明接口：
```cs
public interface ITestRefitClient
{
    [Get("/users/{id}")]
    Task<User> GetUserByIdAsync(int id);
}
```

就这么简单，Refit将自动帮你生成请求代码，并注入到容器中。