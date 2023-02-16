---@class BaseUIBilliardsPrompt : UIGameView
local BaseUIBilliardsPrompt = class("BaseUIBilliardsPrompt", UIGameView)

-- Properties
BaseUIBilliardsPrompt.static.prefabPath = "Game/Billiards/Views/UIBilliardsPrompt.prefab"

function BaseUIBilliardsPrompt:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "vsContent" then com = coms:Get(0)
			elseif k == "player_1" then com = coms:Get(1)
			elseif k == "head_1" then com = coms:Get(2)
			elseif k == "btHead" then com = coms:Get(3)
			elseif k == "lblPlayerName_1" then com = coms:Get(4)
			elseif k == "player_2" then com = coms:Get(5)
			elseif k == "head_2" then com = coms:Get(6)
			elseif k == "btHead1" then com = coms:Get(7)
			elseif k == "lblPlayerName_2" then com = coms:Get(8)
			elseif k == "multyHit" then com = coms:Get(9)
			elseif k == "multyHitHead" then com = coms:Get(10)
			elseif k == "btHead2" then com = coms:Get(11)
			elseif k == "lblHitPlayerName" then com = coms:Get(12)
			elseif k == "lblMuiltHit" then com = coms:Get(13)
			elseif k == "tips" then com = coms:Get(14)
			elseif k == "lblTip" then com = coms:Get(15) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsPrompt