# Hacknet-Pathfinder-KT0Startup



用于kt0的hacknet pathfinder模组



当前版本: 1.2.1

## 灵感来源



`Zero Day toolkit`

`StuxnetHN`

`TempestGadgets`

## 新的自定义exe



- `Java.exe|#JAVA_EXE#` ，用于破解8080端口的shiro服务和389端口的LDAP服务
- `Pwntools.exe|#PWN_EXE#` 用于破解9999端口的Abyss服务
- `EternalBlue.exe|#ETERNALBLUE#` 用于破解445端口的SMB服务
- `Frp.exe|#FRP_EXE#` 用于建立内网隧道，详见[InternalServiceDaemon](https://github.com/Err0r233/Hacknet-Pathfinder-KT0Startup/blob/master/InternalServiceDaemon.md)
- `AutoCrackFirewall.exe|#FIREWALL_AUTO_SOLVER#` solve a firewall automatically after by running this program


## 新的xml标签



* `创建SZip格式文件`

```xml
<!-- 格式1: 用于创建一个文件夹，文件夹下有多个文件、子文件夹、子文件等-->
<!-- 对于子文件夹请使用//作为分隔符 -->
<!-- 如果不需要密码也可以解压，请设置Key的值为Default -->
<SZipFileFolder OutputFolder="bin" ParentFolder="bin" Name="Name.szip" Key="EncryptKey/Default">
    <SZip OutputFileName="xxx" Subfolder="1" Data="fileData"></SZip>
    <SZip OutputFileName="yyy" Subfolder="1" Data="fileData"></SZip>
    <SZip OutputFileName="zzz" Subfolder="1//2" Data="fileData"></SZip>
</SZipFileFolder>
```



```xml
<!-- 格式2: 用于创建一个没有文件夹只有文件的szip文件 -->
<!-- 如果不需要密码也可以解压，请设置Key的值为Default -->
<SZipFile OutputFolder="bin" Name="xxx.szip" Key="EncryptKey/Default">
    <SZip OutputFileName="xxx" Data="fileData"></SZip>
</SZipFile>
```



* `创建内网节点`

```xml
<!-- 警告: 你必须将节点的ip设置成与internalIp一致 -->
<!-- 警告: 你必须将节点的ip设置成与internalIp一致 -->
<!-- 警告: 你必须将节点的ip设置成与internalIp一致 -->
<InternalLink>
   <InternalPC id="PCID" internalIp="10.1.1.1"></InternalPC>
   <InternalPC id="PCID" internalIp="10.1.1.2"></InternalPC>
</InternalLink>
```





## 新命令



- `Base64Encode` 输出一个`Base64`加密后的字符串
- `IScan` 用于扫描内网节点
- `SZip` 用于压缩一个文件、一个文件夹下的所有文件或者用于解压一个szip格式文件。需要添加flag`SZipPermission`才能够正常使用



## 新的守护进程

* `InternalServiceDaemon` 
