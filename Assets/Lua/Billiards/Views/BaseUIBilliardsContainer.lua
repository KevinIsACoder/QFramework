---@class BaseUIBilliardsContainer : UIGameView
local BaseUIBilliardsContainer = class("BaseUIBilliardsContainer", UIGameView)

-- Properties
BaseUIBilliardsContainer.static.prefabPath = "Game/Billiards/Views/UIBilliardsContainer.prefab"

function BaseUIBilliardsContainer:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "content" then com = coms:Get(0)
			elseif k == "panelViewRoot" then com = coms:Get(1)
			elseif k == "headContainer" then com = coms:Get(2)
			elseif k == "popViewRoot" then com = coms:Get(3) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsContainer