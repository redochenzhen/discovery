快速开始
===============

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
      "ConnectionString": "192.168.117.52:2181/services",
      "SessionTimeout": 4000,
      "ConnectionTimeout": 3000,
      "GroupName": "yourgroup",
      //"Password": "123456",

      "Instance": {
        "ServiceName": "yourservicename",
        "Port": 5000,
        "State": "Up", //Down
        "IpAddress": "localhost",
        "PreferIpAddress": true
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