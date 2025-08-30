# Hacknet-Pathfinder-KT0Startup



A Hacknet pathfinder mod for KT0.

[Chinese readme.md](https://github.com/Err0r233/Hacknet-Pathfinder-KT0Startup/blob/master/README-zh-cn.md)



Current Version: 1.2.1

## Inspiration



`Zero Day toolkit`

`StuxnetHN`

`TempestGadgets`

## New executables



- `Java.exe|#JAVA_EXE#` for port 8080 `shiro` and port 389 `LDAP Service`. You should use it with certain `.jar` files
- `RedisExploit.exe|#REDIS_EXE#` for port 6379 Redis
- `Pwntools.exe|#PWN_EXE#` for port 9999 Abyss
- `EternalBlue.exe|#ETERNALBLUE#` for port 445 SMB
- `Frp.exe|#FRP_EXE#` for creating a tunnel between player computer and internal node. Visit [InternalServiceDaemon](https://github.com/Err0r233/Hacknet-Pathfinder-KT0Startup/blob/master/InternalServiceDaemon.md) chapter for more details.
- `AutoCrackFirewall.exe|#FIREWALL_AUTO_SOLVER#` solve a firewall automatically after by running this program




## New Patches



* `Create SZipFile`

```xml
<!-- format 1 for creating folders with subfolders and files-->
<!-- Subfolder should use // for delimitation -->
<!-- If you want to decompress a certain file without password, set the value of "Key" to Default -->
<SZipFileFolder OutputFolder="bin" ParentFolder="bin" Name="Name.szip" Key="EncryptKey/Default">
    <SZip OutputFileName="xxx" Subfolder="1" Data="fileData"></SZip>
    <SZip OutputFileName="yyy" Subfolder="1" Data="fileData"></SZip>
    <SZip OutputFileName="zzz" Subfolder="1//2" Data="fileData"></SZip>
</SZipFileFolder>
```



```xml
<!-- format 2 for creating files without folders -->
<!-- If you want to decompress a certain file without password, set the value of "Key" to Default -->
<SZipFile OutputFolder="bin" Name="xxx.szip" Key="EncryptKey/Default">
    <SZip OutputFileName="xxx" Data="fileData"></SZip>
</SZipFile>
```



* `Create InternalNetwork`

```xml
<!-- Warning: You should set nodes' ip the same with internalIp -->
<!-- Warning: You should set nodes' ip the same with internalIp -->
<!-- Warning: You should set nodes' ip the same with internalIp -->
<InternalLink>
   <InternalPC id="PCID" internalIp="10.1.1.1"></InternalPC>
   <InternalPC id="PCID" internalIp="10.1.1.2"></InternalPC>
</InternalLink>
```





## New Commands



- `Base64Encode` output a base64 encoded string
- `IScan` scan internal nodes
- `SZip` for compress and decompress some file or folder, make it available by adding flag `SZipPermission`



## New Daemons

* `InternalServiceDaemon` 

