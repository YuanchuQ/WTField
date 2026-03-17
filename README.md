# WT Field

一个使用 Unity 制作的 2D 俯视角战斗 demo，围绕一场 3 到 5 分钟的关卡流程展开。项目包含键盘移动、四方向射击、敌人波次、Buff、HUD、音效、胜负结算和中英文 UI 切换，重点呈现从玩法原型到可运行关卡的完整实现过程。

## 项目概览

- 类型：2D 俯视角射击 / 生存战斗
- 引擎：Unity 6000.3.15f1
- 渲染：URP 2D
- 输入：键盘移动、移动方向瞄准、`J` 键射击
- 目标体验：3 到 5 分钟的完整战斗流程

## 操作方式

- `WASD` / 方向键：移动
- 移动方向：决定角色朝向和射击方向
- `J`：射击
- `Enter` / `Space`：开始游戏
- `Esc`：暂停 / 继续
- `R`：结算后重新开始

## 已实现内容

- Tilemap 战斗场景与碰撞边界
- 玩家移动、四方向朝向、射击和受击反馈
- 子弹对象池，减少战斗中的频繁实例化
- 玩家和敌人共用的生命与伤害系统
- 三类敌人：普通追踪、冲刺敌人、精英敌人
- 基于网格的简单 A\* 寻路
- 三波敌人刷新与胜利 / 失败流程
- ScriptableObject 驱动的 Buff 配置
- 治疗、移速、伤害、攻速、护盾、穿透、爆炸、磁吸等 Buff
- HUD、开始菜单、暂停面板、结算面板
- 中英文 UI 切换，默认中文界面
- BGM、SFX 和命中特效

## 技术亮点

- 组件化拆分：玩家、战斗、敌人、Buff、UI 和流程控制分目录管理
- 数据驱动：Buff 和波次使用 ScriptableObject 配置，方便调参和扩展
- 事件驱动：生命变化、Buff 状态、波次状态通过事件同步到 HUD
- 对象池：玩家子弹复用，避免射击时持续创建和销毁对象
- 敌人行为：追踪、冲刺和网格寻路共同构成基础战斗压力
- 本地化：通过 `LanguageManager`、`LocalizedText` 和语言按钮支持中英文切换

## 目录结构

```text
Assets/
  Art/                  美术资源、字体和源图
  Audio/                BGM 与 SFX
  Prefabs/              玩家、敌人、子弹、拾取物和特效预制体
  Scenes/               主场景
  ScriptableObjects/    Buff、波次和 Tile 配置
  Scripts/
    Buffs/              Buff 数据、拾取和效果控制
    Combat/             伤害、生命、子弹和对象池
    Core/               游戏流程、音效和通用工具
    Enemy/              敌人行为、波次和寻路
    Player/             玩家移动、射击和镜头
    UI/                 HUD、本地化和语言切换
```

## 重要文件

- `Assets/Scenes/Main.unity`：可运行主场景
- `Assets/Scripts/Core/GameManager.cs`：开始、暂停、胜负和重开流程
- `Assets/Scripts/Player/PlayerController.cs`：玩家移动、四方向朝向和射击方向
- `Assets/Scripts/Player/PlayerShooter.cs`：射击、攻速和子弹属性
- `Assets/Scripts/Combat/Projectile.cs`：子弹移动、命中、穿透和爆炸
- `Assets/Scripts/Combat/ProjectilePool.cs`：子弹对象池
- `Assets/Scripts/Combat/Damageable.cs`：生命、受伤、治疗和死亡事件
- `Assets/Scripts/Buffs/BuffController.cs`：玩家 Buff 应用和属性刷新
- `Assets/Scripts/Enemy/EnemyController.cs`：敌人追踪、冲刺、攻击和掉落
- `Assets/Scripts/Enemy/GridPathfinder.cs`：网格寻路
- `Assets/Scripts/Enemy/WaveSpawner.cs`：敌人波次和胜利事件
- `Assets/Scripts/UI/LanguageManager.cs`：中英文文本和字体切换

## 运行方式

1. 使用 Unity 6000.3.15f1 或更高版本打开项目
2. 打开 `Assets/Scenes/Main.unity`
3. 点击 Play
4. 按 `Enter` 或点击 `开始作战` 进入战斗

## 鸣谢

感谢 B 站 UP 主 [Voidmatrix](https://space.bilibili.com/25864506/?spm_id_from=333.788.upinfo.detail.click) 提供游戏素材支持。

中文字体使用开源字体 Ark Pixel Font，许可证见 `Assets/Art/Fonts/ArkPixelFont-OFL.txt`。
