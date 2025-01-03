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

注意：生成人物时，可能会在意想不到的的地方生成，比如怀孕生下的孩子或者复制方尖碑（已修复），这可能会造成某些bug。这种情况下可以与我联系，我会修复。

Warming: When generating characters, they might appear in unexpected places, such as a child being born during pregnancy or a duplicated obelisk (now fixed). This could cause some bugs. In such cases, you can contact me, and I will fix it.

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

<!--a simpler syntax-->
<equipment>
    <Gun_BoltActionRifle/>
</equipment>
<inventory>
    <MealSurvivalPack>5<MealSurvivalPack/>
</inventory>
<apparel>
    <Apparel_Pants/>
    <Apparel_BasicShirt/>
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

<!--a simpler syntax-->

<skills>
    <Shooting>(10,0)<Shooting/>
</skills>

<hediffs>
    <Gunshot>(0.5,Head)</Gunshot>
</hediffs>

<traits>
    <Kind/>
    <Nerves>-1<Nerves/>
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

## Comps

### CompTachie

```xml
<--Example-->
<li Class="FixedPawnGenerate.CompProperties_Tachie">
    <texture>path to picture</texture> 
    <offsetY>0</offsetY>
    <offsetX>0</offsetX>
    <scale>1</scale>
</li>
```
该comp可以在角色左下角和信息页添加立绘
左下角的立绘默认显示图片的上部的 3/5, 高度固定是500.
信息页的立绘显示全部，透明度是0.5

texture 字段支持绝对路径

## Scenario

本mod提供一个自定义剧本的功能，只需要如下设置

```xml
<!-- Config pages -->
<li Class="FixedPawnGenerate.ScenPart_ConfigPage_ConfigureStartingFixedPawns"> <--change class-->
    <def>ConfigPage_ConfigureStartingFixedPawns</def> <--change def-->
    <pawnCount>4</pawnCount>
    <pawnChoiceCount>8</pawnChoiceCount>
    <pawnTags> <--add pawnTags-->
        <li/> <--Default-->
        <li>your FixedPawnDef's tag1</li>
        <li>your FixedPawnDef's tag1</li>
        <li>your FixedPawnDef's tag2</li>
        <li>your FixedPawnDef's tag3</li>
        <li>your FixedPawnDef's tag4</li>
        <li>your FixedPawnDef's tag5</li>
        <li>your FixedPawnDef's tag5</li>
    </pawnTags>
</li>
```

需要ScenarioDef的Config pages项。
Class改为"FixedPawnGenerate.ScenPart_ConfigPage_ConfigureStartingFixedPawns"
def改为"ConfigPage_ConfigureStartingFixedPawns"。

pawnTags是开局选人界面对应位置的人物的tag，最好令pawnTags的项的数量与pawnChoiceCount一致
