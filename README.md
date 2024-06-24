# Rimworld-FixedPawnGenerate
可以生成固定人物的mod

A mod that can generate fixed characters

## Introduction
这个mod可以生成固定的世界人物，只需要编写xml配置文件

This mod allows the generation of fixed world characters by simply writing an XML configuration file.

## Usage

开始之前，你需要一些基础的Rimworld Xml mod的基本知识，但也不需要很多，只要会写About.xml和一个Def文件即可。参阅: [About.xml](https://rimworldwiki.com/wiki/Modding_Tutorials/About.xml), [Defs](https://rimworldwiki.com/wiki/Modding_Tutorials/Defs)

关于Def文件，可以参考mod内的ExampleDef.xml。
基本上参数内填的都是对应的Def的defName，例如faction内填的是FactionDef的defName。
race除外，race内是Thingdef的defName，或者说是PawnKindDef的race参数。

所有Def都可以在{Rimworld安装目录}\Data下找到，推荐使用Visual Studio Code，按Ctrl+Shift+F。

Before starting, you need some basic knowledge of Rimworld Xml modding, but not much, just how to write About.xml and a Def file. Refer to: [About.xml](https://rimworldwiki.com/wiki/Modding_Tutorials/About.xml), [Defs](https://rimworldwiki.com/wiki/Modding_Tutorials/Defs)

For Def files, you can refer to the ExampleDef.xml within the mod. 

Basically, the parameters filled in are the corresponding Def's defName, for example, the faction is filled with FactionDef's defName.
Except for race, the race is the defName of Thingdef, or the race parameter of PawnKindDef.

All Defs can be found under {Rimworld installation directory}\Data, it is recommended to use Visual Studio Code, press Ctrl+Shift+F.

### Params

如果参数没有被设置，那么该参数会按照原版逻辑生成。

If the parameter is not set, it will be generated according to the original logic.

#### 1. Match

```xml
<faction>PlayerColony</faction>
<race>Human</race>
<pawnKind>Colonist</pawnKind>
```

生成世界人物时，系统通过这三个参数来匹配，只有匹配成功才可能生成。
匹配失败则按原版逻辑生成。
不设置的字段表示全匹配，例如以下代码表示匹配所有人类

When generating world characters, the system matches through these three parameters, and only generates if the match is successful.
If the match fails, it generates according to the original logic.
Fields not set indicate a full match, for example, the following code matches all humans

```xml
<race>Human</race>
```

#### 2. Generate Rate

```xml
<generateWeight>1</generateWeight>
<generateRate>0.8</generateRate><!--0 - 1-->
<isUnique>false</isUnique> 
```

generateWeight设置匹配到的配置的生成权重，权重越高越可能生成。
generateRate设置匹配到的配置的生成率，0~1之间。
isUnique设置Pawn是否唯一，唯一的Pawn只会被生成一次，例外是开局选人界面。

generateWeight sets the generation weight of the matched configuration, the higher the weight, the more likely it is to generate.
generateRate sets the generation rate of the matched configuration, between 0~1.
isUnique sets whether the Pawn is unique, a unique Pawn will only be generated once, except in the starting character selection interface.

#### 3. Xenotype

```xml
<xenotype MayRequire="ludeon.rimworld.biotech">Pigskin</xenotype>
<customXenotype>testCustomXenotype</customXenotype>
```

需要生物科技DLC。
设置Pawn的xenotype。
customXenotype就是自定义异种的名字
目前customXenotype字段有些bug，有可能匹配不到自定义的异种。

customXenotype比xenotype的优先级高，如果customXenotype没有找到，则使用xenotype

Requires the Biotech DLC.
Sets the Pawn's xenotype.
customXenotype is the name of your custom xenotype
Currently, the customXenotype field has some bugs, it may not match the custom xenotype.

customXenotype has a higher priority than xenotype. If customXenotype is not found, then xenotype will be used.

#### 4. Personal Info

```xml
<age>13.5</age>
<chronologicalAge>200</chronologicalAge>
<gender>2</gender>

<firstName>TestFirstName</firstName>
<nickName>TestNickName</nickName>
<lastName>TestLastName</lastName>
```

age: 生物年龄
chronologicalAge：历法年龄
gender：性别，填Female或Male， 1或2也可以。

firstName，nickName，lastName：角色名字

age: Biological age
chronologicalAge: Chronological age
gender: Gender, fill in Female or Male, 1 or 2 is also possible.

firstName, nickName, lastName: Character's name

#### 5. Head and Body

```xml
<beard>Anchor</beard>
<hair>Mop</hair>
<hairColor>(237, 202, 156)</hairColor>

<headType>Female_AverageWide</headType>
<faceTatoo MayRequire="ludeon.rimworld.ideology">Face_Pantheon</faceTatoo>

<bodyType>Male</bodyType>
<bodyTatoo MayRequire="ludeon.rimworld.ideology">Body_Tiger</bodyTatoo>
<skinColor>(155,155,155)</skinColor>

<childHood>ToxicChild96</childHood>
<adultHood>Archaeologist85</adultHood>

<favoriteColor MayRequire="ludeon.rimworld.ideology">(237, 202, 156)</favoriteColor>
```

hairColor，skinColor，favoriteColor使用RGB颜色
faceTatoo，bodyTatoo，favoriteColor需要文化DLC。

hairColor, skinColor, favoriteColor use RGB colors
faceTatoo, bodyTatoo, favoriteColor require the Ideology DLC.

#### 6. inventory

```xml
<equipment>
    <li>
        <thing>Gun_BoltActionRifle</thing>
    </li>
</equipment>
<inventory>
    <li>
        <thing>MealSurvivalPack</thing>
        <count>5</count>
    </li>
</inventory>
<apparel>
    <li>
        <thing>Apparel_Pants</thing>
    </li>
    <li>
        <thing>Apparel_BasicShirt</thing>
    </li>
</apparel>
```

背包内分为三部分，武器、衣着和持有物。没有设置的部分会依照原版生成，设置了的部分则会覆盖原来的物品。

The inventory is divided into three parts: weapons, clothing, and possessions. Parts not set will be generated according to the original, while set parts will override the original items.

#### 7. Skills、Hediffs、Traits

```xml
    <skills>
    <li>
        <skill>Shooting</skill>
        <level>10</level>
        <passion>0</passion>
    </li>
</skills>

<hediffs>
    <li>
        <hediff>Gunshot</hediff>
        <severity>0.5</severity>
        <bodyPart>Head</bodyPart>
    </li>
</hediffs>

<traits>
    <li>
        <trait>Kind</trait>
    </li>
    <li>
        <trait>Nerves</trait>
        <degree>-1</degree>
    </li>
</traits>
```

技能部分只有定义了的部分会被覆盖
特性方面，如果是没有程度的特性比如Kind(善良),可以省略degree；如果是有程度的特性比如Nerves(意志坚定、意志紧张等)，必须设置degree，因为他们没有degree=0。例如Nerves的degree可以是-1，-2，1，2。

Only the defined parts of skills will be overwritten
For traits, if it's a singular trait like Kind, the degree can be omitted; if it's a spectrum trait like Nerves, the degree must be set because they don't have a degree=0. For example, Nerves's degree can be -1, -2, 1, 2.

#### 8. comps、abilities(Advance)

```xml
<abilities>
    <li>FireSpew</li>
</abilities>

<comps>
    <li Class="your CompProperties class name">
        <!-- 一些compclass的参数 -->
    </li>
</comps>
```

原版游戏中可以通过PawnKindDef为Pawn增加能力，这个的写法是相同的。

原版游戏中似乎没有除了改写<ThingDef>以外的方法为Pawn添加comp。
该mod的写法与原版<TingDef>下<comps>的写法相同，会在Pawn生成时添加comp，不会覆盖原有的comp

这样可以保持原来的race的同时添加comp.

In the original game, abilities can be added to Pawn through PawnKindDef, the writing method is the same.

In the original game, there seems to be no method other than rewriting <ThingDef> to add comp to Pawn.
The writing method of this mod is the same as the <comps> under the original <ThingDef>, it will add comp when Pawn is generated, and will NOT overwrite the original comp

This allows to keep the original race while adding comp.
