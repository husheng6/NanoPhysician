# 绑定动画 + 改变攻击方式

## 现状

- 动画资源已就位：`idle/idle.anim`, `MoveRight/right.anim`, `MoveLeft/left.anim`, `MoveUp/up.anim`, `MoveDown/down.anim`, `Attack/attack.anim`
- 每个文件夹各有独立的 AnimatorController，但需要一个统一的 Controller 来管理所有状态
- `PlayerShooting.cs` 当前用左键（button 0）发射远程子弹
- 没有近战攻击逻辑
- 没有动画驱动脚本

## 计划

### 1. 创建 `PlayerAnimator.cs`（新文件）
路径：`Assets/Scripts/Gameplay/PlayerAnimator.cs`

负责驱动 Animator 组件，根据玩家移动/攻击状态设置动画参数：
- `IsMoving` (bool) — 是否在移动
- `MoveX` / `MoveY` (float) — 移动方向
- `Attack` (trigger) — 触发攻击动画
- `AttackType` (int) — 0=近战, 1=远程

### 2. 创建 `PlayerMeleeAttack.cs`（新文件）
路径：`Assets/Scripts/Gameplay/Combat/PlayerMeleeAttack.cs`

左键近战攻击：
- 在玩家面朝方向前方一定范围内检测 "Enemy" Tag 的碰撞体
- 对检测到的所有敌人造成伤害（范围攻击）
- 触发攻击动画
- 有独立冷却时间

### 3. 修改 `PlayerShooting.cs`
- `aimMouseButton` 从 `0`（左键）改为 `1`（右键）
- 攻击时通知 PlayerAnimator 播放远程攻击动画

### 4. 修改 `PlayerController.cs`
- 添加对 PlayerAnimator 的引用
- 移动时更新动画参数（IsMoving, MoveX, MoveY）

### 5. 创建 Editor 脚本 `PlayerAnimatorSetupEditor.cs`（新文件）
路径：`Assets/Scripts/Editor/PlayerAnimatorSetupEditor.cs`

提供菜单项一键生成统一的 AnimatorController：
- 默认状态：Idle
- Idle → Move（IsMoving=true 时切换）
- Move → Idle（IsMoving=false 时切换）
- Any State → Attack（Attack trigger 触发）
- Attack → Idle（动画播完自动返回）

### 6. 修改 `PlayerStatsConfig.cs`
添加近战攻击相关属性：
- `meleeAttackDamage` — 近战伤害
- `meleeAttackRange` — 近战范围
- `meleeAttackCooldown` — 近战冷却

## 文件变更总结

| 操作 | 文件 |
|------|------|
| 新增 | `Assets/Scripts/Gameplay/PlayerAnimator.cs` |
| 新增 | `Assets/Scripts/Gameplay/Combat/PlayerMeleeAttack.cs` |
| 新增 | `Assets/Scripts/Editor/PlayerAnimatorSetupEditor.cs` |
| 修改 | `Assets/Scripts/Gameplay/Combat/PlayerShooting.cs` |
| 修改 | `Assets/Scripts/Gameplay/PlayerController.cs` |
| 修改 | `Assets/Scripts/Gameplay/Player/PlayerStatsConfig.cs` |
| 修改 | `Assets/Scripts/Gameplay/Player/PlayerStats.cs` |

## Unity Editor 手动步骤（代码完成后）

1. 运行菜单 `NanoPhysician > Create Player Animator Controller` 生成统一 Controller
2. 将生成的 Controller 拖到玩家 GameObject 的 Animator 组件上
3. 给玩家添加 `PlayerAnimator` 和 `PlayerMeleeAttack` 组件
