### 配置详解

|Key|类型|默认值|说明|
|---|---|---|---|
|Discovery|
|:ShouldDiscover|bool|false|是否启用发现功能，一般服务的消费端设置为true|
|:ShouldRegister|bool|false|是否注册为一个服务，一般服务的提供端设置为true|
|:WorkerThreads|int|[cpu核数]|处理Http请求的线程数|
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
|:MaxFails|int|1|在FailTimeout时间内，失败数达到MaxFails，该服务实例被临时冻结|
|:NextWhen|enum[flags]|Error,Timeout|当Http请求发生配置所述状况时，将选择另一个实例重试，可选值为：Never/Error,Timeout,Http500,Http502,Http503,Http403,Http404,NonIdemponent/GetOnly|
|:NextTries|int|0（不限制）|限制重试次数
|:NextTimeout|int|0（不限制）|限制重试时间


