---@class BaseUIBilliardsResult : UIGameView
local BaseUIBilliardsResult = class("BaseUIBilliardsResult", UIGameView)

-- Properties
BaseUIBilliardsResult.static.prefabPath = "Game/Billiards/Views/UIBilliardsResult.prefab"

function BaseUIBilliardsResult:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "winGroup" then com = coms:Get(0)
			elseif k == "lblTitle" then com = coms:Get(1)
			elseif k == "rankList" then com = coms:Get(2)
			elseif k == "rankContent" then com = coms:Get(3)
			elseif k == "rankItem" then com = coms:Get(4)
			elseif k == "btnClose" then com = coms:Get(5)
			elseif k == "btnAgain" then com = coms:Get(6) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsResult