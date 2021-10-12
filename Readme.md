# No Component File Uploader For Unity

这是一个基于无惧上传类修改的 Unity 适配的无组件上传类及其测试工程。

# 功能演示
![](doc/uploader.gif)

# Summary
1. 无需后台人员支持，也不用安装其他网页开发环境
2. 修复 UnityWebRequest 在构建二进制上传数据意外在头部加入了换行符导致的上传不成功的bug
3. 新增指定上传目录的功能，请自己做校验避免用户构建并上传到敏感文件夹

# How to use?

1. Unity 开启工程并点击播放按钮 ，详见 gif 
2. 请确保你的 PC 80端口未被占用（如占用，改 MyWebServer 端口 和 上传 url）
