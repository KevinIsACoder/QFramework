---@class BaseUIBilliardsSelect : UIGameView
local BaseUIBilliardsSelect = class("BaseUIBilliardsSelect", UIGameView)

-- Properties
BaseUIBilliardsSelect.static.prefabPath = "Game/Billiards/Views/UIBilliardsSelect.prefab"

function BaseUIBilliardsSelect:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "btnClose" then com = coms:Get(0)
			elseif k == "btnRule" then com = coms:Get(1)
			elseif k == "toggleGroup" then com = coms:Get(2)
			elseif k == "togNormal" then com = coms:Get(3)
			elseif k == "offstate" then com = coms:Get(4)
			elseif k == "onStateNormal" then com = coms:Get(5)
			elseif k == "togEasy" then com = coms:Get(6)
			elseif k == "offstate1" then com = coms:Get(7)
			elseif k == "onStateEasy" then com = coms:Get(8)
			elseif k == "gatherTipButton" then com = coms:Get(9) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsSelect