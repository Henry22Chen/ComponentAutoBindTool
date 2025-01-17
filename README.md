
# ComponentAutoBindTool
0202年了作为底层拼图仔的你还在Unity里手写组件绑定代码？快来试试这款船新的组件自动绑定工具吧，只需三分钟，你就会爱上它。

实现了editor时期使用字符串，runtime时期使用索引获取组件，以消除不必要的内存分配。

支持自定义自动绑定规则，提供了默认的自动绑定规则，根据物体命名前缀进行组件识别，支持单个物体的多组件识别绑定。

基本用法：为要进行自动绑定物体挂载ComponentAutoBindTool脚本，根据选择的绑定规则要求修改物体相关信息，点击自动绑定组件，然后设置自动生成的绑定代码的命名空间，类名与保存路径，最后点击生成绑定代码即可。

AutoBindGlobalSetting.asset为默认设置文件，可放置于Asset目录下任意位置，若不慎丢失可通过点击菜单栏的CatWorkflow/CreateAutoBindGlobalSetting进行创建。

本项目基于 https://github.com/egametang/ET 中的 ReferenceCollector 开发。

---

## 主要变化

1. 按自己项目的命名规范更新了代码。
2. 新增 `AutoBindBehaviour` 以及 `IAutoBindPage` 以支持在 Inspector 上显示绑定的脚本引用。
3. 新增 `AutoBindableAttribute` 以支持在自定义组件上标记添加到组件绑定映射表。
4. 使用 [generic-serializable-dictionary](https://github.com/upscalebaby/generic-serializable-dictionary) 进行序列化，而不依赖数组顺序。
5. 支持 Unity Package Manager 安装。

## 依赖

- [generic-serializable-dictionary](https://github.com/upscalebaby/generic-serializable-dictionary)

## 安装

> 需要提前安装依赖，或者使用 [Git Dependency Resolver For Unity](https://github.com/mob-sakai/GitDependencyResolverForUnity) 自动安装 git 依赖。

- 使用 Unity Package Manager 安装：选择左上角  "+" 号，点击 "Add package from git URL..."，输入 `https://github.com/Henry22Chen/ComponentAutoBindTool.git`
- 或者手动编辑 `manifest.json`，添加 `"com.henrychen.component-bind-tool": "https://github.com/Henry22Chen/ComponentAutoBindTool.git"`
