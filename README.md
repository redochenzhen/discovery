# **Keep.Discovery**

## 简介
[Keep.Discovery](https://github.com/redochenzhen/discovery)是基于asp.net core平台的服务发现框架。目前具有以下特性和限制：

### 特性
1. 基于抽象实现，方便扩展支持其他热门服务发现组件，如Consul, Eureka等
2. 拥有可靠的容灾能力
3. 对开发完全透明，请求由标准的HttpClient完成
4. 内置了“静态发现”功能，方便本地调试
5. 可与Refit集成，只需申明接口，自动生成Client代码

### 限制（暂时）
1. 只支持Restful Api (http协议)，不支持WCF, gRPC等rpc协议
2. 只支持基于ZooKeeper实现的ZooPicker服务发现组件

### 同步Submodule
* git clone https://github.com/redochenzhen/discovery.git
* cd discovery
* git submodule update --init

### 内部文档
[http://docs.kede.net/discovery/](http://docs.kede.net/discovery/)

### 构建文档
* 下载[docfx](https://github.com/dotnet/docfx/releases)<br />
* 将docfx.exe所在路径添加到path环境变量<br />
* 在项目根目录执行：<br />
docfx<br />
docfx serve _site<br />

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
        "NextTries": 0,
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

### 配置详解

|Key|类型|默认值|说明|
|---|---|---|---|
|Discovery|
|:ShouldDiscover|bool|false|是否启用发现功能，一般服务的消费端设置为true|
|:ShouldRegister|bool|false|是否注册为一个服务，一般服务的提供端设置为true|
|:WorkerThreads|int|[cpu核数]|ZooPicker处理Http请求的线程|
|:DefaultRequestTimeout|int|100,000|Http请求默认超时时间，有别于HttpClient.Timeout，请[参见](http://docs.kede.net/discovery/articles/tips.html#requesttimeout)|
|:PathMatch|string|/discovery|服务实例信息查询的路由配置|
|:Mapping|array\<json\>|[]|用于静态服务发现的本地映射配置，与"PathMatch"路由下查询获得的数据结构相同|
|Discovery:ZooPicker|
|:ConnectionString|string|localhost:2181/services|ZooKeeper链接字符串|
|:SessionTimeout|int|6,000|ZooKeeper会话过期时间，单位：ms|
|:ConnectionTimeout|int|20,000|连接ZooKeeper服务的超时时间，单位：ms|
|:GroupName|string|Default|服务分组名称，用于组织管理服务
|:Password|string||访问ZooKeeper中，该分组节点的密码（暂未支持）|
|Discovery:ZooPicker:Instance|
|:ServiceName|string|无/必填|服务名称|
|:Type|enum|Rest|服务类型，可选值：Rest/Grpc/Wcf（除Rest之外，其它暂不支持）|
|:Secure|bool|false|scheme是否为https
|:Port|int|80/443|服务端口，默认使用http或https的默认端口|
|:Balancing|enum|RoundRobin|负载均衡策略，可选值为：RoundRobin/Random|
|:IpAddress|string|127.0.0.1|用此IP地址覆盖真实的IP地址，当PreferIpAddress=true时有效|
|:PreferIpAddress|bool|true|使用IP地址代替HostName|
|:Weight|int|1|负载均衡权重 [变更立即有效]|
|:State|enum|Up|服务状态，可选值为：Up/Down/Backup（Back暂不支持）  [变更立即有效]|
|:FailTimeout|int|10,000|失败超时时间，当服务实例因失败而被临时冻结后，每隔FailTimeout将获得一次尝试机会，单位：ms|
|:MaxFails|int|1|在FailTimeout时间内，失败数达到MaxFails，该服务实例被临时冻结，单位：ms|
|:NextWhen|enum[flags]|Error,Timeout|当Http请求发生配置所述状况时，将选择另一个实例重试，可选值为：Never/Error,Timeout,Http500,Http502,Http503,Http403,Http404,NonIdemponent/GetOnly|
|:NextTries|int|0（不限制）|限制重试次数
|:NextTimeout|int|0（不限制）|限制重试时间

