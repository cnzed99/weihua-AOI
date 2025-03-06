<div><center><b>
    <font color="34,63,93" size="7"> 
        软件配置
    </font>
</b></center></div>

# 1、appConfig.json

```
{
  "APP": {
    "Config": {
      "hasMarkConfig": true,
      "hasFocusConfig": true,
      "IsDark": true
    }
  }
}
```

hasMarkConfig：控制打标模块是否存在，每个产品检测结束时执行相应函数，通过传递编码器值精确定位NG位置，在NG到达时执行动作；；

hasFocusConfig：自动对焦功能、运动控制等功能块（基于PLC的通讯，监控和设参作用）；

IsDark：颜色主题
