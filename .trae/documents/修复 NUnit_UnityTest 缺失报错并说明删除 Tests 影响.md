## 结论（原因）
- Assets/Tests 没有 .asmdef，测试脚本被编进默认 Assembly-CSharp；默认程序集不引用 Unity Test Framework 的 NUnit/TestRunner 程序集，所以出现“找不到 NUnit / Test / UnityTest”。

## 执行方案（按推荐方案）
1. 新增 EditMode 测试程序集定义
   - 新建 `Assets/Tests/EditMode/VSL.Tests.EditMode.asmdef`
   - includePlatforms: `Editor`
   - references: `UnityEditor.TestRunner`, `UnityEngine.TestRunner`
   - optionalUnityReferences: `TestAssemblies`
2. 新增 PlayMode 测试程序集定义
   - 新建 `Assets/Tests/PlayMode/VSL.Tests.PlayMode.asmdef`
   - references: `UnityEngine.TestRunner`（如编辑器仍提示缺引用，再补 `UnityEditor.TestRunner`）
   - optionalUnityReferences: `TestAssemblies`
3. 触发 Unity 重新导入后复查编译
   - 确认 `LocalizationCsvTests.cs` 与 `LocalizationSwitchTests.cs` 的 NUnit/UnityTest 相关错误全部消失
4. 再跑一遍全项目诊断
   - 若有后续编译错误（例如其他脚本引用缺失类型），继续逐个定位并修复