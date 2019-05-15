--主入口函数。从这里开始lua逻辑
function Main()					
	print("logic start")	 		
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage("collect")
	Time.timeSinceLevelLoad = 0
	local GameObject = UnityEngine.GameObject
	local ParticleSystem = UnityEngine.ParticleSystem

	local go = GameObject('go')
	go:AddComponent(typeof(ParticleSystem))
	local node = go.transform
	node.position = Vector3.one
end

function OnApplicationQuit()
end