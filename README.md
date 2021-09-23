# FlourLed_UpCom
四色光源上位机程序
# 版本



## V1.9.3 2021-09-22

- 修改亮度等级由0~5到 1~5



## v1.9.2     2021-09-22

完成了所有逻辑
- 增加了全亮按钮

- 增加了电压电流 与发光时长的对应关系（电压电流根据发光时长的变化而变化）

- 常亮状态下不能点亮两种及两种以上的灯珠。

  

  | 发光时间                         | 电流值 | 电压值 | 亮度值 |
  | -------------------------------- | ------ | ------ | ------ |
  | 500ns                            | 10     | 45     | x      |
  | 3000us    >    F    >      500ns | 10     | 10     | x      |
  | 5000us    >    F    >=   3000us  | 10~20  | 10     | 1~5    |
  | 10000us  >  F    >=    5000us    | 12~20  | 35     | 1~5    |
  | 50ms  >  F    >=    10ms         | 12~20  | 30     | 1~5    |
  | 1s  >  F    >=    50ms           | 15~20  | 25     | 1~5    |
  | 蓝光或绿光常亮                   | 12~15  | 30     | 1~5    |
  | 红光或黄光常亮                   | 12~15  | 20     | 1~5    |

  

## v1.9.1     2021-09-18
改变了部分逻辑（还没有写完）


## v1.5 2021-06-02
增加了常亮电源、闪烁电源切换的逻辑判断。

## v1.4.1
2021-05-14   16:38   修复了第四通道与第三通道数据引用错误的问题

## v1.4   
2020-12-18    11:48    修改了数据协议，都以3个字节为一个通道

## v.1.3   
2020-12-09  15:08     修改bug：周期数最大值的限制

## v1.2    
2020-11-24  15:54     增加：发光时间用小数点表示，优化算法

## v1.1    
2020-11-24  10:29     增加：读取单片机的参数

## v1.0    
2020-11-24  09:20     初始版本