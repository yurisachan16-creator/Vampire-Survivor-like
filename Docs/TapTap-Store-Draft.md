# TapTap 商店页草稿

本草稿基于当前项目真实功能编写，已经按国区加海外、全球预约页、5 种现有语言支持的口径整理为可直接填写后台的版本。

## 商店元信息矩阵

| 字段 | 值 |
| --- | --- |
| `store_scope` | 国区加海外 |
| `region_strategy` | 全球预约页 |
| `contact_email` | `yurisachan16@gmail.com` |
| `platform_rollout_note` | Android 首发，Windows 随后开放 |
| `privacy_summary` | 单机、无账号、无内购、无联机，仅处理本地存档、设置、本地排行榜、崩溃或错误日志 |

## 多语言字段矩阵

| `display_name` | `subtitle` | `platform_rollout_note` |
| --- | --- | --- |
| 简体中文：`夜幕幸存者` | `在夜幕怪潮中清场升级，构筑属于你的幸存者流派。` | `预约中，Android 首发，Windows 随后开放。` |
| 繁體中文：`夜幕倖存者` | `在夜幕怪潮中清場升級，構築屬於你的倖存者流派。` | `預約中，Android 首發，Windows 隨後開放。` |
| English：`Nightfall Survivors` | `Clear the night swarm, level up fast, and forge your own survivor build.` | `Pre-registration is open. Android launches first, with Windows coming later.` |
| 日本語：`宵闇サバイバーズ` | `夜の怪潮を切り開き、成長しながら自分だけのビルドを作り上げよう。` | `事前登録受付中。Android 先行、Windows は後日対応予定。` |
| 한국어：`암야의 생존자들` | `밤의 괴물 물결을 돌파하고 성장하며 나만의 생존자 빌드를 완성하세요.` | `사전 예약 진행 중이며 Android 먼저 출시되고 Windows는 추후 지원됩니다.` |

## 国区与 International 口径

### 国区

- 主地区：中国大陆
- 主语言：简体中文
- 繁中如后台支持，可作为补充说明，不作为主字段

### International

- 页面策略：全球预约页
- 主文案语言：English
- 补充语言：繁體中文、日文、韩文
- 简中不作为 International 主展示语言

## `language_region_priority`

### 中国区

| 语言 | 国家/地区 | 说明 |
| --- | --- | --- |
| `zh-Hans` | 中国大陆 | 国区主页面语言 |

### International 重点国家/地区

| 语言 | 国家/地区 | 用途 |
| --- | --- | --- |
| `zh-Hant` | 台湾、香港、澳门 | 繁中页面与素材优先覆盖 |
| `en` | 美国、加拿大、英国、澳大利亚、新西兰、新加坡、菲律宾、马来西亚、印度 | 英文主版与首轮全球预约页重点 QA 市场 |
| `ja` | 日本 | 日文页面与素材优先覆盖 |
| `ko` | 韩国 | 韩文页面与素材优先覆盖 |

### International 收缩顺序

如果后台要求首批开放地区不能直接全开，则按下面顺序收缩：

1. 台湾、香港、澳门
2. 日本、韩国
3. 美国、加拿大、英国、澳大利亚、新西兰
4. 新加坡、菲律宾、马来西亚、印度

## 简体中文长简介

`夜幕幸存者` 是一款 2D 幸存者动作游戏。你将在不断增强的怪潮中移动、拾取、升级与进化武器，用越来越完整的构筑撑过整场战斗。

### 核心特色

1. 自动攻击与走位生存并重，在高密度敌群中打出节奏。
2. 升级三选一，持续构筑属于自己的战斗流派。
3. 宝箱进化带来关键质变，让一局中的成长更有爆发感。
4. 金币永久成长、成就与本地排行榜形成长期目标。
5. 当前已支持简中、繁中、英文、日文、韩文。

### 当前内容

- 波次推进与 Boss 压力测试
- 多种武器、被动与掉落物组合
- 宝箱合成、成就、永久升级与本地排行榜
- 触屏操作支持，适配 Android 游玩

### 当前说明

- 当前为预约/关注页
- Android 首发
- Windows 稍后开放
- 单机、无账号、无内购、无联机

## English Description

`Nightfall Survivors` is a 2D survivor action game built around movement, crowd control, and rapid build decisions. Fight through escalating night swarms, collect drops, level up through three-choice upgrades, and evolve your weapons into stronger forms as the run unfolds.

### Highlights

1. Fast movement-driven survival with auto-attacks
2. Three-choice level-up decisions that shape every run
3. Treasure chest evolutions that unlock key power spikes
4. Persistent upgrades, achievements, and a local leaderboard for long-term goals
5. Android launches first, with Windows coming later
6. Offline single-player only, with no account system, no multiplayer, and no in-app purchase

## 日本語紹介文

`宵闇サバイバーズ` は、移動判断とビルド構築を軸にした 2D サバイバーアクションゲームです。押し寄せる怪潮の中で移動、回収、三択レベルアップ、武器進化を重ねながら生き残ります。

### 特徴

1. オート攻撃と立ち回りの両方が重要
2. レベルアップ三択で毎回ビルドが変化
3. 宝箱進化で戦力が大きく伸びる
4. 永続強化、実績、ローカルランキングで継続目標を用意
5. Android 先行、Windows は後日対応予定
6. オフライン専用、アカウントなし、課金なし、マルチプレイなし

## 한국어 소개문

`암야의 생존자들`은 이동 판단과 빌드 조합을 중심으로 한 2D 서바이버 액션 게임입니다. 밤의 괴물 물결 속에서 이동, 수집, 3지선다 레벨업, 무기 진화를 거듭하며 끝까지 살아남아야 합니다.

### 핵심 특징

1. 자동 공격과 생존형 이동 플레이를 함께 요구하는 전투
2. 레벨업 3지선다로 매 판 다른 빌드 구성
3. 보물상자 진화로 전투 흐름이 크게 바뀌는 성장 구조
4. 영구 성장, 업적, 로컬 랭킹으로 이어지는 장기 목표
5. Android 선출시, Windows는 이후 지원
6. 오프라인 싱글 플레이 전용, 계정 없음, 멀티플레이 없음, 인앱 결제 없음

## 繁體中文介紹文

`夜幕倖存者` 是一款以走位與構築為核心的 2D 倖存者動作遊戲。你需要在夜幕怪潮中持續移動、撿取掉落、完成升級選擇，並透過寶箱進化打造更完整的戰鬥流派。

### 核心特色

1. 自動攻擊與生存走位並重
2. 升級三選一，逐步構築自己的戰鬥流派
3. 寶箱進化提供關鍵質變與爆發成長
4. 金幣永久成長、成就與本地排行榜構成長線目標
5. Android 先行，Windows 隨後開放
6. 單機離線、無帳號、無內購、無多人連線

## 平台与版本说明

- Android：首发平台
- Windows：稍后开放
- 联网需求：无强制联网
- 账号系统：无
- 付费形态：当前按无付费版本准备

## 关键词建议

- 幸存者
- Roguelite
- 动作
- 割草
- Build 构筑
- 自动攻击
- 宝箱进化
- Boss 战
- 本地成长
