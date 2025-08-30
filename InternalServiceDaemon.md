# English Version

When setting  tag `<InternalServiceDaemon />` for a node, this node cannot be connected. You should establish tunnel by `frp.exe`



You can build a internal network group via the following steps:

1. set `InternalLink` for a node. Player can use`IScan` on this node to discover `internal nodes`.
2. `InteralLink's internalIp` tag will be used to judge `frp` command is right or not. for example, when `internalIp` is set to `10.1.1.1` or `10.1.1.2`, type `frp -c 10.1.1.0` to establish connection to `10.1.1.1-255`.
3. set `InternalServiceDaemon` for `internal nodes`.



# Chinese Version

当为节点设置了`<InternalServiceDaemon />`标签后，节点会变为`内网节点`。内网节点无法直接访问，必须通过`frp.exe`建立内网连接后才可以访问。



你需要搭配以下的步骤才能够实现一套完整的内网访问流程：

1. 为一个节点设置`InternalLink`标签，通过这个标签玩家可以使用`IScan`扫描出内网机器
2. `internalIp`的c段会被用于建立frp连接，例如`frp -c 10.1.1.0`用于建立从`10.1.1.1-255`段的连接
3. 为被扫描出来的节点设置`<InternalServiceDaemon />`