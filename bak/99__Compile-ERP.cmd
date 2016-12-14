
rem 代码规范检查工具专用脚本，请勿删除！

pushd ..

echo 正在获取所有ERP代码......
"%VS140COMNTOOLS%\..\IDE\tf.exe" get *.* /recursive /noprompt

popd

rem echo 去Bin只读……
attrib "../00 根目录/bin/*.*" -r /S


set msbuildexe=C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe

echo COPY平台程序集……
robocopy "../99 packages/运行平台" "../00 根目录/bin" *.dll *.pdb /e /nfl /ndl /np
robocopy "../99 packages/建模平台" "../00 根目录/bin" *.dll *.pdb /e /nfl /ndl /np

echo COPY公共系统……
rem attrib "../99 packages/公共系统/*.*" -r /S
rem attrib "../99 packages/产品接口/公共接口/*.*" -r /S
robocopy "../99 packages/公共系统" "../00 根目录/bin" *.dll *.pdb /e /nfl /ndl /np
robocopy "../99 packages/产品接口/公共接口" "../00 根目录/bin" *.dll *.pdb /e /nfl /ndl /np
robocopy "../99 packages/Quartz.2.3.1/lib/net40-client" "../00 根目录/bin" Quartz.dll /e /nfl /ndl /np


echo 正在生成项目库……
"%msbuildexe%"  "../01 项目库/项目库.sln" /t:Rebuild /p:Configuration="Debug" /consoleloggerparameters:ErrorsOnly /nologo /m

echo 发布项目库接口文件 ...
rem del "..\99 packages\产品接口\公共接口\Mysoft.PubPlatform.ProjectOverview.*.*" /q
rem robocopy "../00 根目录/bin" "../99 packages/产品接口/公共接口" Mysoft.PubPlatform.ProjectOverview.PublicServices.dll Mysoft.PubPlatform.ProjectOverview.Model.dll /e /nfl /ndl /np

echo 正在生成主数据系统……
"%msbuildexe%"  "../04 主数据系统/主数据系统.sln" /t:Rebuild /p:Configuration="Debug" /consoleloggerparameters:ErrorsOnly /nologo /m
rem echo 发布主数据接口文件 ...
rem del "..\99 packages\产品接口\主数据接口\*.*" /q
rem robocopy "../00 根目录/bin" "../99 packages/产品接口/主数据接口" Mysoft.Mdm.*.Interfaces.dll Mysoft.Mdm.*.Model.dll /e /nfl /ndl /np

echo 正在生成售楼系统……
"%msbuildexe%"  "../02 售楼系统/售楼系统.sln" /t:Rebuild /p:Configuration="Debug" /consoleloggerparameters:ErrorsOnly /nologo /m
rem echo 发布售楼接口文件 ...
rem del "..\99 packages\产品接口\售楼接口\*.*" /q
rem robocopy "../00 根目录/bin" "../99 packages/产品接口/售楼接口" Mysoft.Slxt.*.interfaces.dll Mysoft.Slxt.*.Model.dll /e /nfl /ndl /np

echo 正在生成成本系统……
"%msbuildexe%"  "../03 成本系统/成本系统.sln" /t:Rebuild /p:Configuration="Debug" /consoleloggerparameters:ErrorsOnly /nologo /m
rem echo 发布成本接口文件 ...
rem del "..\99 packages\产品接口\成本接口\*.*" /q
rem robocopy "../00 根目录/bin" "../99 packages/产品接口/成本接口" Mysoft.Cbxt.*.interfaces.dll Mysoft.Cbxt.*.Model.dll /e /nfl /ndl /np
