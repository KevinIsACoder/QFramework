---@class BaseUIBilliardsRule : UIGameView
local BaseUIBilliardsRule = class("BaseUIBilliardsRule", UIGameView)

-- Properties
BaseUIBilliardsRule.static.prefabPath = "Game/Billiards/Views/UIBilliardsRule.prefab"

function BaseUIBilliardsRule:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "lblRule" then com = coms:Get(0)
			elseif k == "btnClose" then com = coms:Get(1)
			elseif k == "scrollRect" then com = coms:Get(2)
			elseif k == "lblRule_1" then com = coms:Get(3)
			elseif k == "lblSolidBall" then com = coms:Get(4)
			elseif k == "lblStripBall" then com = coms:Get(5)
			elseif k == "lblRule_2" then com = coms:Get(6)
			elseif k == "lblRule_3" then com = coms:Get(7)
			elseif k == "lblRule_4" then com = coms:Get(8)
			elseif k == "lblRule_5" then com = coms:Get(9)
			elseif k == "lblRule_6" then com = coms:Get(10)
			elseif k == "lblRule_7" then com = coms:Get(11)
			elseif k == "lblNormal" then com = coms:Get(12)
			elseif k == "lblEasy" then com = coms:Get(13)
			elseif k == "lblRule_8" then com = coms:Get(14) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsRule