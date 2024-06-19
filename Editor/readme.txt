注意：
这个脚本是针对项目 

需要按需调整：
	Meta48Environment.AreaType = OUTDOOR ？
	Meta48Environment.m_environmenType = Building/Physics ？

规则：
	导入文件名匹配 [SM_/Sm]_*_fileName_*_[LOD*/collider].FBX/fbx	
	预制体生成位置：
		导入位置文件夹向上搜索 "Meshes" 文件夹 	或者 未搜索到	
		1.在Meshes文件夹同级的 “_Prefabs” 下 同导入路径文件夹处		然后生成预制体
		2.没有Meshes文件夹 会在导入位置自动创建“_Prefabs”文件夹 	然后生成预制体

导入文件 	可单次，多次，增加， 导入一个/多个模型。 
修改模型	去Windows文件夹手动替换FBX文件
删除模型	不支持自动功能：需要手动调整 P_,Sm_x_LOD*,Sm_x_collider 的结构和引用
	
执行功能：
	1.【自动】配置模型导入设置	// 客户方添加了自己有模型处理设置 所以不再自动设置
								// 但添加了 文件夹 或者 模型多选 	【可手动：	Assets/AssetTool/修正选中模型设置】
	2.【自动】材质球导入自动配置									【可手动：	Assets/AssetTool/修正选中Material】								
	2.【自动】预制体文件生成位置检查
	3.【自动】自动生成配置预制体									【可手动：	Assets/AssetTool/创建选中资源的预制件】
	4.【自动】资源规范 名字规范 位置 旋转 缩放规范检查 				【可手动：	Assets/AssetTool/检查资源规范【名字|Transform】】
	5.【手动】重置选中[Regula和Model]预制件Transform				【手动：	Assets/AssetTool/重置预制件【Regular和Model】Transform】
	
但需要注意 Untiy重复文件会自动添加编号
预制件生成位置会自动检查对应文件夹 所以同组文件导入需要在同一文件夹下



-----------------------------------------------Don't read---------------------------------------------------

例： 导入文件 SM_XXXX_Test_LOD0 SM_XXXX_Test_LOD1 SM_XXXX_Test_LOD2 SM_XXXX_Test_collider

自动化步骤
自动设置模型导入设置:
	模型	Mode		=>	Standard
	模型	Location	=>	Use External Materials(Legacy)
	模型	Naming		=>	From Models Material
	模型	Search		=>	Project-Wide

自动创建预制件：
	P_XXXX_Test							
		P_XXXX_Test_LOD
			P_XXXX_Test_LOD0			=> 预制件引用
			P_XXXX_Test_LOD1			=> 预制件引用
			P_XXXX_Test_LOD2			=> 预制件引用
		P_XXXX_Test_collider
			P_XXXX_Test_collider		=> 预制件引用
	P_XXXX_Test_LOD0
	P_XXXX_Test_LOD1
	P_XXXX_Test_LOD2
	P_XXXX_Test_collider

自动调整预制体 P_XXXX_Test
	P_XXXX_Test_LOD
		添加组件	Meta48Environment
		调整组件	Meta48Environment
						LodGroup => 
							P_XXXX_Test_LOD0
							P_XXXX_Test_LOD1
							P_XXXX_Test_LOD2
						AreaType = OUTDOOR
						m_environmenType = Building
		添加组件	LODGroup
		调整组件	LODGroup
						LOD 0/1/2 = 70/60/0
	P_XXXX_Test_collider
		添加组件	Meta48Environment
		调整组件	Meta48Environment
						LodGroup => P_XXXX_Test_collider
						AreaType = OUTDOOR
						m_environmenType = Physics
	
自动调整预制体 P_XXXX_Test_LOD0/P_XXXX_Test_LOD1/P_XXXX_Test_LOD2
	调整组件	MeshRenderer
					LightProbes => Off					
	
自动调整预制体 P_XXXX_Test_collider 对象：
	删除组件	MeshFilter
	删除组件	MeshRenderer
	添加组件	MeshCollider	[自动引用原Mesh]
	调整组件	Layer => Physics_World_Default


