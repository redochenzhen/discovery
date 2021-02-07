必读贴士
===============

* ### RequestTimeout
我们知道，Discovery框架会向服务容器中注入HttpClient实例，HttpClient包含[Timeout属性](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.timeout?view=netcore-3.1)和[SendAsync方法](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.sendasync?view=netcore-3.1)
<br /><br />
比如设置Timeout为3s，请求如果不能在3秒内返回，将会抛出TaskCancledException
<br /><br />
再来看SendAsync方法，它的有些重载中可接受一个CancellationToken参数，允许用户在必要时主动取消本次请求，同时抛出TaskCancledException
<br /><br />
框架内部的HttpMessageHandler无法区分是什么原因导致http请求被取消了，从而无法根据这个TaskCancledException来判断是否需要Timeout重试
<br /><br />
所以，不要依赖HttpClinet.Timeout来超时你的请求，保持其默认值（100m）不变，而必须依赖Discovery内部实现的超时机制
<br /><br />
全局默认配置：
```cs
services
    .AddDiscovery(options =>
    {
        options.DefaultRequestTimeout = 5000;
        options.UseZooPicker();
    });
```
配置指定的HttpClient:
```cs
services
    .AddDiscovery(options =>
    {
        options.UseZooPicker();
    })
    .AddDiscoveryHttpClient<ITestClient, TestClient>()
    .ConfigureHttpMessageHandlerBuilder(builder =>
    {
        builder.SetDefaultTimeout(6000);
    })
```
如果你的粒度是要控制到某个请求，请在把HttpMessageRequest请求对象传递给HttpClient.SendAsync之前，调用其扩展方法SetTimeout（在Keep.Common包中提供）
```cs
request.SetTimeout(TimeSpan.FromSeconds(10));
```
这样，请求超时后会抛出TimeoutException。如果你的配置Discovery:ZooPicker:Instance:Next:When中包含了Timeout枚举，框架就能正确的执行重试策略
<br /><br />
如果你使用了Refit（Client代码自动生成），请使用PropertyAttribute来设置超时时间。值得注意的是，PropertyAttribute在Refit包6.0.0+(preview)版本中才提供，且该版本要求使用Net5。
```cs
[Get("/users/{id}")]
Task<User> GetUserAsync(int id, [Property(TimeoutHandler.TIMEOUT_KEY)] TimeSpan requestTimeout);
```
***************