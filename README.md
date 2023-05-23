# FileStorage


适用于AbpBoilerplate的本地文件存储模块。





## 快速开始


在项目中引用[AbpBoilerplate.FileStorage]( https://www.nuget.org/packages/AbpBoilerplate.FileStorage)


```
dotnet add package AbpBoilerplate.FileStorage
```

添加FileStorageModule模块依赖
```
[DependsOn(typeof(FileStorageModule))]
public class CoreModule : AbpModule

```

appsettings.json配置文件中，添加服务相关配置
```
"FileStorage": {
    "DirectorySeparator": "/",          //文件分隔符
    "RetainUnusedBlobs": false,         //在删除文件后是否删除物理文件
    "EnableAutoRename": true,           //重名时是否自动重命名，而不是报错
    "MaxByteSizeForEachFile": 1024,     //最大文件
    "MaxByteSizeForEachUpload": 4096,   //最大上传文件体积，单位GB
    "MaxFileQuantityForEachUpload": 2,  //上传最大线程数
    "AllowOnlyConfiguredFileExtensions": false, //是否只允许上传已配置的文件扩展名
    "FileExtensionsConfiguration": ".jpg,.png",//配置允许上传文件的扩展名
    "GetDownloadInfoTimesLimitEachUserPerMinute": 10    //限制每分钟访问下载信息的最大次数
}
...

```



## 使用帮助




## 作者信息

作者：林小

邮箱：jevonsflash@qq.com



## License

The MIT License (MIT)
