---@class BaseUIBilliardsMenu : UIGameView
local BaseUIBilliardsMenu = class("BaseUIBilliardsMenu", UIGameView)

-- Properties
BaseUIBilliardsMenu.static.prefabPath = "Game/Billiards/Views/UIBilliardsMenu.prefab"

function BaseUIBilliardsMenu:OnCreated()
	UIGameView.OnCreated(self)
	local coms = self.component:GetComponents(self.transform, true)
	self.view = setmetatable({}, {
		__index = function(t, k)
			local com = nil
			if k == "ruleBtn" then com = coms:Get(0)
			elseif k == "gameFeedBackBtn" then com = coms:Get(1)
			elseif k == "stopMusicBtn" then com = coms:Get(2)
			elseif k == "openMusicBtn" then com = coms:Get(3) end
			rawset(t, k, com)
			return com
		end
	})
end

return BaseUIBilliardsMenu